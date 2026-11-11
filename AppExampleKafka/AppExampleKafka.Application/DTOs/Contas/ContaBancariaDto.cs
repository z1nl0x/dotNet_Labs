namespace AppExampleKafka.Application.DTOs.Contas;

public record ContaBancariaDto(Guid Id, decimal Saldo, string Moeda, bool Ativa,
    string Agencia, string Numero, string DigitoVerificador, string TipoConta, DateTime CriadoEm);
