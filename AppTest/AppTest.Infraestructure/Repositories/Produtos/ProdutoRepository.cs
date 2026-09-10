using AppTest.Domain.Entities.Produtos;
using AppTest.Domain.Repositories;
using AppTest.Infraestructure.Context;
using Microsoft.EntityFrameworkCore;

namespace AppTest.Infraestructure.Repositories.Produtos;

public class ProdutoRepository(AppDbContext context) : IProdutoRepository
{
    public Task<Produto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Produtos
            .Include(p => p.Categoria)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<Produto> Items, int TotalCount)> ListAsync(
        string? nome,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = context.Produtos
            .AsNoTracking()
            .Include(p => p.Categoria)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(nome))
            query = query.Where(p => EF.Functions.ILike(p.Nome, $"%{nome.Trim()}%"));

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(p => p.Nome)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<bool> AnyByCategoryAsync(Guid categoriaId, CancellationToken cancellationToken = default) =>
        context.Produtos.AnyAsync(p => p.CategoriaId == categoriaId, cancellationToken);

    public async Task AddAsync(Produto produto, CancellationToken cancellationToken = default) =>
        await context.Produtos.AddAsync(produto, cancellationToken);

    public void Remove(Produto produto) => context.Produtos.Remove(produto);
}
