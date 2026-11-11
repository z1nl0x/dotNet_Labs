using AppExampleKafka.Application.Common.Interfaces;

namespace AppExampleKafka.Tests;

public class GatePublisher : IEventoPublisher
{
    public TaskCompletionSource Entered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public int Calls { get; private set; }
    public async Task PublicarAsync(Guid eventId, Guid transacaoId, string payload, CancellationToken ct)
    {
        Calls++;
        Entered.TrySetResult();
        await Release.Task.WaitAsync(TimeSpan.FromSeconds(15), ct);
    }
}
