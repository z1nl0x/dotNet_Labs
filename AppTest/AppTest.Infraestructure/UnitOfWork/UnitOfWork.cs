using AppTest.Domain.Repositories;
using AppTest.Infraestructure.Context;

namespace AppTest.Infraestructure.UnitOfWork;

/// <summary>
/// Implementação do Unit of Work sobre o <see cref="AppDbContext"/>.
/// Expõe os repositórios e centraliza o commit das alterações.
/// </summary>
public class UnitOfWork(
    AppDbContext context,
    IUserRepository users,
    IRefreshTokenRepository refreshTokens,
    ICategoriaRepository categorias,
    IProdutoRepository produtos) : IUnitOfWork
{
    public IUserRepository Users { get; } = users;

    public IRefreshTokenRepository RefreshTokens { get; } = refreshTokens;

    public ICategoriaRepository Categorias { get; } = categorias;

    public IProdutoRepository Produtos { get; } = produtos;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);
}
