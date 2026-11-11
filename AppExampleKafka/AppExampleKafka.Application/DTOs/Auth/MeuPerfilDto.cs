using AppExampleKafka.Application.DTOs.Usuarios;

namespace AppExampleKafka.Application.DTOs.Auth;

public record MeuPerfilDto(Guid Id, string Nome, string Email, string Role, CadastroBancarioDto? Cadastro);
