namespace AppExampleKafka.Application.DTOs.Usuarios;

public record DadosPessoaFisicaDto(DateOnly DataNascimento, string Nacionalidade, string EstadoCivil, string Profissao, decimal RendaMensal);
