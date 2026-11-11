namespace AppExampleKafka.Application.Common.Interfaces;

public interface IEventoPublisher
{
    Task PublicarAsync(Guid eventId, Guid transacaoId, string payload, CancellationToken ct);
}
