using AppExampleKafka.Domain.Entities.Common;
using System.Security.Cryptography;

namespace AppExampleKafka.Domain.Entities.Bancario;

public class ContaBancaria : BaseEntity
{
    public Guid UsuarioId { get; private set; }
    public string Agencia { get; private set; } = "0001";
    public string Numero { get; private set; } = string.Empty;
    public string DigitoVerificador { get; private set; } = string.Empty;
    public string TipoConta { get; private set; } = "Corrente";
    public decimal Saldo { get; private set; }
    public string Moeda { get; private set; } = "BRL";
    public bool Ativa { get; private set; } = true;
    public void TransferirPara(ContaBancaria destino, decimal valor)
    {
        if (valor <= 0 || valor != decimal.Round(valor, 2) || valor > 9999999999999999.99m)
            throw new ArgumentException("Valor deve ser positivo, até 16 dígitos inteiros e 2 casas decimais.");
        if (Id == destino.Id || !Ativa || !destino.Ativa || Moeda != destino.Moeda)
            throw new InvalidOperationException("As contas devem ser distintas, ativas e da mesma moeda.");
        if (Saldo < valor) throw new InvalidOperationException("Saldo insuficiente.");
        if (destino.Saldo > 9999999999999999.99m - valor)
            throw new InvalidOperationException("Saldo máximo da conta de destino excedido.");
        Saldo -= valor;
        destino.Saldo += valor;
        AtualizadoEm = destino.AtualizadoEm = DateTime.UtcNow;
    }

    private ContaBancaria() { }
    public ContaBancaria(Guid usuarioId)
    {
        if (usuarioId == Guid.Empty) throw new ArgumentException("Titular obrigatório.");
        UsuarioId = usuarioId;
        Numero = string.Concat(Enumerable.Range(0, 12).Select(_ => RandomNumberGenerator.GetInt32(10)));
        // Módulo 11: pesos 2..9 da direita para a esquerda; restos 0/1 produzem DV 0.
        var soma = Numero.Reverse().Select((digito, indice) => (digito - '0') * (2 + indice % 8)).Sum();
        var resto = soma % 11;
        DigitoVerificador = (resto < 2 ? 0 : 11 - resto).ToString();
    }
}
