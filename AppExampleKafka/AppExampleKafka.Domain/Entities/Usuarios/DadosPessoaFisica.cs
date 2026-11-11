namespace AppExampleKafka.Domain.Entities.Usuarios;

public record DadosPessoaFisica(DateOnly DataNascimento, string Nacionalidade, string EstadoCivil, string Profissao, decimal RendaMensal);
