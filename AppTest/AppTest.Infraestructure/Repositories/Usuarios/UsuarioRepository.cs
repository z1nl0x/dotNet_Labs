using AppTest.Domain.Entities.Usuarios;
using AppTest.Domain.Repositories;
using AppTest.Infraestructure.Context;
using Microsoft.EntityFrameworkCore;

namespace AppTest.Infraestructure.Repositories.Users;

public class UsuarioRepository(AppDbContext context) : IUserRepository
{
    public Task<Usuario?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<Usuario?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        context.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default) =>
        context.Users.AnyAsync(u => u.Email == email, cancellationToken);

    public async Task AddAsync(Usuario usuario, CancellationToken cancellationToken = default) =>
        await context.Users.AddAsync(usuario, cancellationToken);
}
