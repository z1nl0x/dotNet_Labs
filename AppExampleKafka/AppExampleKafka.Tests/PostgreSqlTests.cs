using AppExampleKafka.Application;
using AppExampleKafka.Application.Common.Exceptions;
using AppExampleKafka.Application.Common.Interfaces;
using AppExampleKafka.Application.Features.Contas.Create;
using AppExampleKafka.Application.Features.Transacoes.Create;
using AppExampleKafka.Domain.Entities.Bancario;
using AppExampleKafka.Domain.Entities.Usuarios;
using AppExampleKafka.Infraestructure;
using AppExampleKafka.Infraestructure.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace AppExampleKafka.Tests;

public class PostgreSqlFactAttribute : FactAttribute
{
    public PostgreSqlFactAttribute()
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPEXAMPLEKAFKA_TEST_POSTGRES"))) Skip = "Defina APPEXAMPLEKAFKA_TEST_POSTGRES para a base descartável do compose (porta 15437).";
    }
}

public class KafkaPostgreSqlFactAttribute : PostgreSqlFactAttribute
{
    public KafkaPostgreSqlFactAttribute()
    {
        if (Environment.GetEnvironmentVariable("APPEXAMPLEKAFKA_TEST_KAFKA") != "true") Skip = "Defina APPEXAMPLEKAFKA_TEST_KAFKA=true para usar o broker local do compose.";
    }
}

public sealed class PostgreSqlFixture : IAsyncDisposable
{
    private readonly string schema = "banking_test_" + Guid.NewGuid().ToString("N");
    private readonly string adminConnection;
    public ServiceProvider Provider { get; }
    public Guid UsuarioId { get; } = Guid.NewGuid();
    public Guid OrigemId { get; private set; }
    public Guid DestinoId { get; private set; }
    public SaveBarrier Barrier { get; } = new();
    public PostgreSqlFixture()
    {
        adminConnection = Environment.GetEnvironmentVariable("APPEXAMPLEKAFKA_TEST_POSTGRES")!;
        var cs = new NpgsqlConnectionStringBuilder(adminConnection);
        if (cs.Port is 5432 or 5437 || cs.Database != "apptest_bancario") throw new InvalidOperationException("Use exclusivamente apptest_bancario em porta isolada (15437), nunca a base existente.");
        using (var conn = new NpgsqlConnection(adminConnection))
        {
            conn.Open();
            using var command = new NpgsqlCommand($"CREATE SCHEMA {schema}", conn);
            command.ExecuteNonQuery();
        }
        cs.SearchPath = schema;
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddApplication();
        services.AddInfrastructure(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?> { ["ConnectionStrings:DefaultConnection"] = cs.ConnectionString }).Build());
        services.AddDbContext<AppDbContext>(o => o.AddInterceptors(Barrier));
        services.AddScoped<IUsuarioAtual>(_ => new TestUser { Id = UsuarioId });
        Provider = services.BuildServiceProvider();
    }
    public async Task InicializarAsync()
    {
        using var scope = Provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
        var usuario = new Usuario("Ana", "ana@example.com", "hash");
        db.Entry(usuario).Property(x => x.Id).CurrentValue = UsuarioId;
        var outro = new Usuario("Bia", "bia@example.com", "hash");
        db.Users.AddRange(usuario, outro);
        var origem = new ContaBancaria(UsuarioId);
        var destino = new ContaBancaria(outro.Id);
        db.Contas.AddRange(origem, destino);
        db.Entry(origem).Property(x => x.Saldo).CurrentValue = 100m;
        await db.SaveChangesAsync();
        OrigemId = origem.Id;
        DestinoId = destino.Id;
    }
    public async ValueTask DisposeAsync()
    {
        await Provider.DisposeAsync();
        await using var conn = new NpgsqlConnection(adminConnection);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand($"DROP SCHEMA {schema} CASCADE", conn);
        await cmd.ExecuteNonQueryAsync();
    }
}

public sealed class SaveBarrier : SaveChangesInterceptor
{
    public bool Enabled { get; set; }
    private int arrivals;
    private readonly TaskCompletionSource ready = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (Enabled && eventData.Context!.ChangeTracker.Entries<Transacao>().Any(e => e.State == EntityState.Added))
        {
            if (Interlocked.Increment(ref arrivals) == 2) ready.TrySetResult();
            await ready.Task.WaitAsync(TimeSpan.FromSeconds(15), cancellationToken);
        }
        return result;
    }
}

public class PostgreSqlTests
{
    [PostgreSqlFact]
    [Trait("Category", "Integration")]
    public async Task Mesma_chave_concorrente_debita_uma_vez_e_retry_retorna_original()
    {
        await using var f = new PostgreSqlFixture();
        await f.InicializarAsync();
        f.Barrier.Enabled = true;
        var command = new CriarTransacaoCommand(f.OrigemId, f.DestinoId, 10m, "mesma-chave-concorrente");
        async Task<Guid?> Transferir()
        {
            using var scope = f.Provider.CreateScope();
            try { return (await scope.ServiceProvider.GetRequiredService<ISender>().Send(command)).Id; }
            catch (ConflictException) { return null; }
        }
        var resultados = await Task.WhenAll(Transferir(), Transferir());
        Assert.Contains(resultados, id => id.HasValue);
        using var check = f.Provider.CreateScope();
        var db = check.ServiceProvider.GetRequiredService<AppDbContext>();
        var original = await db.Transacoes.AsNoTracking().SingleAsync();
        Assert.All(resultados.Where(id => id.HasValue), id => Assert.Equal(original.Id, id));
        var retry = await check.ServiceProvider.GetRequiredService<ISender>().Send(command);
        Assert.Equal(original.Id, retry.Id);
        Assert.Equal(90m, (await db.Contas.AsNoTracking().SingleAsync(c => c.Id == f.OrigemId)).Saldo);
        Assert.Equal(10m, (await db.Contas.AsNoTracking().SingleAsync(c => c.Id == f.DestinoId)).Saldo);
        Assert.Single(await db.Transacoes.AsNoTracking().ToListAsync());
        Assert.Single(await db.Outbox.AsNoTracking().ToListAsync());
    }

    [PostgreSqlFact]
    [Trait("Category", "Integration")]
    public async Task Migration_Inicial_cria_identificacoes_unicas_e_permite_reaplicacao()
    {
        await using var f = new PostgreSqlFixture();
        using var scope = f.Provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var migrator = db.GetService<Microsoft.EntityFrameworkCore.Migrations.IMigrator>();
        await migrator.MigrateAsync();
        Assert.EndsWith("_Inicial", Assert.Single(await db.Database.GetAppliedMigrationsAsync()));
        var usuario = new Usuario("Ana", "migration@example.com", "hash");
        db.Entry(usuario).Property(c => c.Id).CurrentValue = f.UsuarioId;
        db.Users.Add(usuario);
        await db.SaveChangesAsync();
        var ids = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        var aberto = new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        foreach (var id in ids)
        {
            var conta = new ContaBancaria(usuario.Id);
            db.Entry(conta).Property(c => c.Id).CurrentValue = id;
            db.Entry(conta).Property(c => c.Saldo).CurrentValue = 12.34m;
            db.Entry(conta).Property(c => c.CriadoEm).CurrentValue = aberto;
            db.Contas.Add(conta);
        }
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var contas = await db.Contas.AsNoTracking().ToListAsync();
        Assert.Equal(ids.Order(), contas.Select(c => c.Id).Order());
        Assert.Equal(3, contas.Select(c => c.Numero).Distinct().Count());
        foreach (var conta in contas)
        {
            Assert.Equal("0001", conta.Agencia);
            Assert.Equal("Corrente", conta.TipoConta);
            Assert.Matches("^[0-9]{12}$", conta.Numero);
            var soma = conta.Numero.Reverse().Select((c, i) => (c - '0') * (2 + i % 8)).Sum();
            Assert.Equal((soma % 11 < 2 ? 0 : 11 - soma % 11).ToString(), conta.DigitoVerificador);
            Assert.Equal(aberto, conta.CriadoEm);
            Assert.Equal(12.34m, conta.Saldo);
            Assert.Equal(usuario.Id, conta.UsuarioId);
        }
        var nova = await scope.ServiceProvider.GetRequiredService<ISender>().Send(new CriarContaCommand());
        Assert.DoesNotContain(nova.Numero, contas.Select(c => c.Numero));
        var duplicada = new ContaBancaria(usuario.Id);
        db.Entry(duplicada).Property(c => c.Numero).CurrentValue = contas[0].Numero;
        db.Contas.Add(duplicada);
        await Assert.ThrowsAsync<ConflictException>(() => scope.ServiceProvider.GetRequiredService<AppExampleKafka.Domain.Repositories.IUnitOfWork>().SaveChangesAsync());
        db.ChangeTracker.Clear();
        await migrator.MigrateAsync("0");
        Assert.Empty(await db.Database.GetAppliedMigrationsAsync());
        await migrator.MigrateAsync();
        Assert.EndsWith("_Inicial", Assert.Single(await db.Database.GetAppliedMigrationsAsync()));
        Assert.Empty(await db.Users.AsNoTracking().ToListAsync());
        Assert.Empty(await db.Contas.AsNoTracking().ToListAsync());
    }

    [KafkaPostgreSqlFact]
    [Trait("Category", "Integration")]
    public async Task Worker_publica_evento_commitado_em_Kafka_real_e_confirma_outbox()
    {
        await using var f = new PostgreSqlFixture();
        await f.InicializarAsync();
        Guid eventId;
        Guid transacaoId;
        using (var scope = f.Provider.CreateScope())
        {
            var transacao = await scope.ServiceProvider.GetRequiredService<ISender>().Send(new CriarTransacaoCommand(f.OrigemId, f.DestinoId, 10m, "kafka-real"));
            transacaoId = transacao.Id;
            var evento = await scope.ServiceProvider.GetRequiredService<AppDbContext>().Outbox.AsNoTracking().SingleAsync();
            eventId = evento.Id;
            Assert.Null(evento.PublicadoEm);
        }
        using var consumer = new Confluent.Kafka.ConsumerBuilder<string,string>(new Confluent.Kafka.ConsumerConfig
        {
            BootstrapServers = "localhost:9092", GroupId = "appexamplekafka-check-" + Guid.NewGuid(), AutoOffsetReset = Confluent.Kafka.AutoOffsetReset.Earliest,
            EnableAutoCommit = false, AllowAutoCreateTopics = false
        }).Build();
        consumer.Subscribe("banco.transacoes.v1");
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var worker = f.Provider.GetServices<Microsoft.Extensions.Hosting.IHostedService>().Single(s => s is AppExampleKafka.Infraestructure.Messaging.OutboxWorker);
        await worker.StartAsync(timeout.Token);
        try
        {
            var received = await Task.Run(() =>
            {
                while (true)
                {
                    var item = consumer.Consume(timeout.Token);
                    using var json = System.Text.Json.JsonDocument.Parse(item.Message.Value);
                    if (json.RootElement.GetProperty("EventId").GetGuid() == eventId) return item;
                }
            }, timeout.Token);
            Assert.Equal(transacaoId.ToString(), received.Message.Key);
            Assert.Contains(eventId.ToString(), received.Message.Value);
            for (var i = 0; i < 50; i++)
            {
                using var check = f.Provider.CreateScope();
                var stored = await check.ServiceProvider.GetRequiredService<AppDbContext>().Outbox.AsNoTracking().SingleAsync(timeout.Token);
                if (stored.PublicadoEm is not null) return;
                await Task.Delay(100, timeout.Token);
            }
            Assert.Fail("Worker recebeu ACK mas não confirmou a outbox.");
        }
        finally { await worker.StopAsync(CancellationToken.None); consumer.Close(); }
    }

    [PostgreSqlFact]
    [Trait("Category", "Integration")]
    public async Task Outbox_nao_publica_linhas_nao_commitadas()
    {
        await using var f = new PostgreSqlFixture();
        await f.InicializarAsync();
        using var producerScope = f.Provider.CreateScope();
        var db = producerScope.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var transaction = await db.Database.BeginTransactionAsync();
        var origem = await db.Contas.SingleAsync(c => c.Id == f.OrigemId);
        var destino = await db.Contas.SingleAsync(c => c.Id == f.DestinoId);
        origem.TransferirPara(destino, 1m);
        var transacao = new Transacao(f.UsuarioId, origem, destino, 1m, "nao-commitado");
        db.Transacoes.Add(transacao);
        db.Outbox.Add(new OutboxMensagem(Guid.NewGuid(), transacao.Id, "{}"));
        await db.SaveChangesAsync();
        using var workerScope = f.Provider.CreateScope();
        var publisher = new TestPublisher();
        var dispatcher = new AppExampleKafka.Infraestructure.Messaging.OutboxDispatcher(workerScope.ServiceProvider.GetRequiredService<AppDbContext>(), publisher);
        Assert.False(await dispatcher.PublicarProximaAsync(default));
        Assert.Empty(publisher.Ids);
        await transaction.CommitAsync();
        Assert.True(await dispatcher.PublicarProximaAsync(default));
        Assert.Single(publisher.Ids);
    }

    [PostgreSqlFact]
    [Trait("Category", "Integration")]
    public async Task Workers_concorrentes_nao_publicam_simultaneamente_a_mesma_mensagem()
    {
        await using var f = new PostgreSqlFixture();
        await f.InicializarAsync();
        using (var create = f.Provider.CreateScope())
            await create.ServiceProvider.GetRequiredService<ISender>().Send(new CriarTransacaoCommand(f.OrigemId, f.DestinoId, 1m, "workers"));
        using var one = f.Provider.CreateScope();
        using var two = f.Provider.CreateScope();
        var publisher = new GatePublisher();
        var first = new AppExampleKafka.Infraestructure.Messaging.OutboxDispatcher(one.ServiceProvider.GetRequiredService<AppDbContext>(), publisher);
        var second = new AppExampleKafka.Infraestructure.Messaging.OutboxDispatcher(two.ServiceProvider.GetRequiredService<AppDbContext>(), publisher);
        var publishing = first.PublicarProximaAsync(default);
        await publisher.Entered.Task.WaitAsync(TimeSpan.FromSeconds(10));
        try { Assert.False(await second.PublicarProximaAsync(default)); }
        finally { publisher.Release.TrySetResult(); }
        Assert.True(await publishing);
        Assert.Equal(1, publisher.Calls);
    }

    [PostgreSqlFact]
    [Trait("Category", "Integration")]
    public async Task Duas_transferencias_concorrentes_nao_podem_gastar_o_mesmo_saldo()
    {
        await using var f = new PostgreSqlFixture();
        await f.InicializarAsync();
        f.Barrier.Enabled = true;
        async Task<bool> Transferir(string chave)
        {
            using var scope = f.Provider.CreateScope();
            try
            {
                await scope.ServiceProvider.GetRequiredService<ISender>().Send(new CriarTransacaoCommand(f.OrigemId, f.DestinoId, 80m, chave));
                return true;
            }
            catch (ConflictException) { return false; }
        }
        var resultados = await Task.WhenAll(Transferir("concorrente-1"), Transferir("concorrente-2"));
        Assert.Single(resultados, x => x);
        using var check = f.Provider.CreateScope();
        var db = check.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Equal(20m, (await db.Contas.SingleAsync(c => c.Id == f.OrigemId)).Saldo);
        Assert.Equal(80m, (await db.Contas.SingleAsync(c => c.Id == f.DestinoId)).Saldo);
        Assert.Single(await db.Transacoes.ToListAsync());
        Assert.Single(await db.Outbox.ToListAsync());
    }
}
