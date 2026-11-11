namespace AppExampleKafka.Domain.Entities.Bancario;

public class OutboxMensagem
{
    public Guid Id { get; private set; }
    public Guid TransacaoId { get; private set; }
    public string Payload { get; private set; } = string.Empty;
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;
    public DateTime? PublicadoEm { get; private set; }
    private OutboxMensagem() { }
    public OutboxMensagem(Guid id, Guid transacaoId, string payload)
    {
        Id = id;
        TransacaoId = transacaoId;
        Payload = payload;
    }
    public void MarcarPublicado() => PublicadoEm = DateTime.UtcNow;
}
