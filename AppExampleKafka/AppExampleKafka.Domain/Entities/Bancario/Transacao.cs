using AppExampleKafka.Domain.Entities.Common;

namespace AppExampleKafka.Domain.Entities.Bancario;

/// <summary>Transferência interna imutável. Não existe exclusão de histórico.</summary>
public class Transacao : BaseEntity
{
    public Guid UsuarioId { get; private set; }
    public Guid ContaOrigemId { get; private set; }
    public Guid ContaDestinoId { get; private set; }
    public decimal Valor { get; private set; }
    public string Moeda { get; private set; } = "BRL";
    public string ChaveIdempotencia { get; private set; } = string.Empty;
    private Transacao() { }
    public Transacao(Guid usuarioId, ContaBancaria origem, ContaBancaria destino, decimal valor, string chave)
    {
        UsuarioId = usuarioId;
        ContaOrigemId = origem.Id;
        ContaDestinoId = destino.Id;
        Valor = valor;
        Moeda = origem.Moeda;
        ChaveIdempotencia = chave;
    }
}
