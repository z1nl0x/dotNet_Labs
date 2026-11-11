using AppExampleKafka.Domain.Entities.Bancario;

namespace AppExampleKafka.Domain.Repositories;

public interface IContaBancariaRepository
{
    Task AddAsync(ContaBancaria conta, CancellationToken ct);
    Task<ContaBancaria?> GetAsync(Guid id, CancellationToken ct);
    Task<(IReadOnlyList<ContaBancaria> Items, int Total)> ListAsync(Guid usuarioId, int page, int size, CancellationToken ct);
}
