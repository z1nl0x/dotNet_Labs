using System.Text.Json;
using AppExampleKafka.Application;
using AppExampleKafka.Application.Features.Auth.Register;
using AppExampleKafka.Application.DTOs.Auth;
using AppExampleKafka.Domain.Entities.Bancario;
using AppExampleKafka.Domain.Entities.Usuarios;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;

namespace AppExampleKafka.Tests;

public class MappingTests
{
    [Fact]
    public void Perfis_registrados_sao_validos_e_preservam_JSON_bancario()
    {
        using var provider = new ServiceCollection().AddApplication().BuildServiceProvider();
        var mapper = provider.GetRequiredService<IMapper>();
        mapper.ConfigurationProvider.AssertConfigurationIsValid();
        var origem = new ContaBancaria(Guid.NewGuid());
        var destino = new ContaBancaria(Guid.NewGuid());
        var transacao = new Transacao(origem.UsuarioId, origem, destino, 10m, "mapping");
        var assembly = typeof(RegisterCommand).Assembly;
        var contaDto = assembly.GetType("AppExampleKafka.Application.DTOs.Contas.ContaBancariaDto", true)!;
        var transacaoDto = assembly.GetType("AppExampleKafka.Application.DTOs.Transacoes.TransacaoDto", true)!;
        Assert.Equal(JsonSerializer.Serialize(new { origem.Id, origem.Saldo, origem.Moeda, origem.Ativa,
            origem.Agencia, origem.Numero, origem.DigitoVerificador, origem.TipoConta, origem.CriadoEm }),
            JsonSerializer.Serialize(mapper.Map(origem, typeof(ContaBancaria), contaDto), contaDto));
        Assert.Equal(JsonSerializer.Serialize(new { transacao.Id, transacao.ContaOrigemId, transacao.ContaDestinoId,
            transacao.Valor, transacao.Moeda, transacao.CriadoEm }),
            JsonSerializer.Serialize(mapper.Map(transacao, typeof(Transacao), transacaoDto), transacaoDto));
    }

    [Fact]
    public void Contratos_de_cadastro_e_perfil_nao_expoem_entidades_de_dominio()
    {
        var types = new Queue<Type>([typeof(RegisterCommand), typeof(MeuPerfilDto)]);
        var visited = new HashSet<Type>();
        while (types.TryDequeue(out var type))
        {
            if (!visited.Add(type)) continue;
            foreach (var property in type.GetProperties())
            {
                Assert.False(property.PropertyType.Namespace?.StartsWith("AppExampleKafka.Domain.Entities") == true,
                    $"{type.Name}.{property.Name} expõe {property.PropertyType.Name}");
                if (property.PropertyType.Namespace?.StartsWith("AppExampleKafka.Application.DTOs") == true)
                    types.Enqueue(property.PropertyType);
            }
        }
    }
}
