namespace AppExampleKafka.Application.DTOs.Transacoes;

public record TransacaoDto(Guid Id, Guid ContaOrigemId, Guid ContaDestinoId, decimal Valor, string Moeda, DateTime CriadoEm);
