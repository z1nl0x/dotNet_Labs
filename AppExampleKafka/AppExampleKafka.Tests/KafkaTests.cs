using AppExampleKafka.Infraestructure.Messaging;
using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace AppExampleKafka.Tests;

public class KafkaTests
{
    [Fact]
    public async Task Infraestrutura_registra_worker_e_publisher_reais()
    {
        await using var f = new BankingFixture();
        var hosted = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetServices<Microsoft.Extensions.Hosting.IHostedService>(f.Provider);
        Assert.Contains(hosted, s => s is OutboxWorker);
        using var publisher = (KafkaEventoPublisher)Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<AppExampleKafka.Application.Common.Interfaces.IEventoPublisher>(f.Provider);
        Assert.NotNull(publisher);
    }

    [Fact]
    public void Producer_exige_ack_de_todas_as_replicas_e_idempotencia()
    {
        var config = KafkaEventoPublisher.CriarConfiguracao(new KafkaOptions());
        Assert.True(config.EnableIdempotence);
        Assert.Equal(Acks.All, config.Acks);
        Assert.False(config.AllowAutoCreateTopics);
        Assert.Equal("localhost:9092", config.BootstrapServers);
    }
}
