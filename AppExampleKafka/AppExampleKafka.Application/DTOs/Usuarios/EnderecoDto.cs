namespace AppExampleKafka.Application.DTOs.Usuarios;

public record EnderecoDto(string Logradouro, string Numero, string Bairro, string Cidade, string Uf, string Cep, string? Complemento = null);
