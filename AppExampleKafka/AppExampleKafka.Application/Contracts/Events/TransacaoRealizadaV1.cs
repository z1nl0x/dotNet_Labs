namespace AppExampleKafka.Application.Contracts.Events;

public record TransacaoRealizadaV1(Guid EventId, int Versao, string Tipo, Guid TransacaoId, Guid ContaOrigemId, Guid ContaDestinoId, decimal Valor, string Moeda, DateTime OcorridoEm);
