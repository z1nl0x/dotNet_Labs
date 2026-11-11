using AppExampleKafka.Domain.Entities.Usuarios;
using AppExampleKafka.Domain.Repositories;
using AppExampleKafka.Infraestructure.Context;
using Microsoft.EntityFrameworkCore;

namespace AppExampleKafka.Infraestructure.Repositories.Users;

public class UsuarioRepository(AppDbContext context) : IUserRepository
{
    public Task<bool> DocumentoExistsAsync(string documento, CancellationToken cancellationToken = default) =>
        context.Users.AnyAsync(u => u.Cadastro != null && u.Cadastro.Documento == documento, cancellationToken);
    public Task<Usuario?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<Usuario?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        context.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default) =>
        context.Users.AnyAsync(u => u.Email == email, cancellationToken);

    public async Task AddAsync(Usuario usuario, CancellationToken cancellationToken = default) =>
        await context.Users.AddAsync(usuario, cancellationToken);
}
