namespace AppExampleKafka.Domain.Entities.Usuarios;

public record Endereco(string Logradouro, string Numero, string Bairro, string Cidade, string Uf, string Cep, string? Complemento = null);
