using System.Text;
using AppExampleKafka.Application.Common.Interfaces;
using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace AppExampleKafka.Infraestructure.Messaging;

public sealed class KafkaEventoPublisher : IEventoPublisher, IDisposable
{
    private readonly IProducer<string, string> producer;
    private readonly string topic;
    public KafkaEventoPublisher(IOptions<KafkaOptions> options)
    {
        topic = options.Value.Topic;
        producer = new ProducerBuilder<string, string>(CriarConfiguracao(options.Value)).Build();
    }
    public static ProducerConfig CriarConfiguracao(KafkaOptions options) => new()
    {
        BootstrapServers = options.BootstrapServers,
        EnableIdempotence = true,
        Acks = Acks.All,
        AllowAutoCreateTopics = false,
        MessageTimeoutMs = options.MessageTimeoutMs,
        SecurityProtocol = options.SecurityProtocol,
        SaslMechanism = options.SaslMechanism,
        SaslUsername = options.SaslUsername,
        SaslPassword = options.SaslPassword,
        SslCaLocation = options.SslCaLocation,
        ClientId = "appexamplekafka-bancario-outbox"
    };
    public async Task PublicarAsync(Guid eventId, Guid transacaoId, string payload, CancellationToken ct)
    {
        await producer.ProduceAsync(topic, new Message<string, string>
        {
            Key = transacaoId.ToString(), Value = payload,
            Headers = new Headers { { "event-id", Encoding.UTF8.GetBytes(eventId.ToString()) }, { "event-type", Encoding.UTF8.GetBytes("TransacaoRealizada.v1") } }
        }, ct);
    }
    public void Dispose() => producer.Dispose();
}
