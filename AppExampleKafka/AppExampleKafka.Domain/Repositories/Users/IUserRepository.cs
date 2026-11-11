using AppExampleKafka.Domain.Entities.Usuarios;

namespace AppExampleKafka.Domain.Repositories;

/// <summary>
/// Contrato de acesso a dados para a entidade <see cref="Usuarios"/>.
/// </summary>
public interface IUserRepository
{
    Task<bool> DocumentoExistsAsync(string documento, CancellationToken cancellationToken = default);
    Task<Usuario?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Usuario?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);

    Task AddAsync(Usuario usuarios, CancellationToken cancellationToken = default);
}
