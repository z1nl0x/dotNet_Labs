using AppExampleKafka.Domain.Entities.Bancario;
using AppExampleKafka.Domain.Repositories;
using AppExampleKafka.Infraestructure.Context;
using Microsoft.EntityFrameworkCore;

namespace AppExampleKafka.Infraestructure.Repositories;

public class ContaBancariaRepository(AppDbContext db) : IContaBancariaRepository
{
    public async Task AddAsync(ContaBancaria conta, CancellationToken ct) => await db.Contas.AddAsync(conta, ct);
    public Task<ContaBancaria?> GetAsync(Guid id, CancellationToken ct) => db.Contas.FirstOrDefaultAsync(c => c.Id == id, ct);
    public async Task<(IReadOnlyList<ContaBancaria> Items, int Total)> ListAsync(Guid usuarioId, int page, int size, CancellationToken ct)
    {
        var query = db.Contas.AsNoTracking().Where(c => c.UsuarioId == usuarioId);
        return (await query.OrderBy(c => c.CriadoEm).ThenBy(c => c.Id).Skip((page - 1) * size).Take(size).ToListAsync(ct), await query.CountAsync(ct));
    }
}
