using AppTest.Domain.Entities;
using AppTest.Domain.Entities.Categorias;
using AppTest.Domain.Repositories;
using AppTest.Infraestructure.Context;
using Microsoft.EntityFrameworkCore;

namespace AppTest.Infraestructure.Repositories.Categorias;

public class CategoriaRepository(AppDbContext context) : ICategoriaRepository
{
    public Task<Categoria?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Categorias.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<Categoria> Items, int TotalCount)> ListAsync(
        string? nome,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = context.Categorias
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(nome))
            query = query.Where(c => EF.Functions.ILike(c.Nome, $"%{nome.Trim()}%"));

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(c => c.Nome)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Categorias.AnyAsync(c => c.Id == id, cancellationToken);

    public async Task AddAsync(Categoria categoria, CancellationToken cancellationToken = default) =>
        await context.Categorias.AddAsync(categoria, cancellationToken);

    public void Remove(Categoria categoria) => context.Categorias.Remove(categoria);
}
