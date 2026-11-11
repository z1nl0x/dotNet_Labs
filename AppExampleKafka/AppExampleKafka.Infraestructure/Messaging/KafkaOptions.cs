using System.Text;
using AppExampleKafka.Application.Common.Interfaces;
using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace AppExampleKafka.Infraestructure.Messaging;

public class KafkaOptions
{
    public const string SectionName = "Kafka";
    public bool Enabled { get; set; } = true;
    public string BootstrapServers { get; set; } = "localhost:9092";
    public string Topic { get; set; } = "banco.transacoes.v1";
    public int PollIntervalSeconds { get; set; } = 5;
    public int MessageTimeoutMs { get; set; } = 10000;
    public SecurityProtocol SecurityProtocol { get; set; } = SecurityProtocol.Plaintext;
    public SaslMechanism? SaslMechanism { get; set; }
    public string? SaslUsername { get; set; }
    public string? SaslPassword { get; set; }
    public string? SslCaLocation { get; set; }
}
