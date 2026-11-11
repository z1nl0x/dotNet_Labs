namespace AppExampleKafka.Application.DTOs.Usuarios;

public record DadosPessoaJuridicaDto(string RazaoSocial, string NomeFantasia, DateOnly DataConstituicao, string NaturezaJuridica, string Cnae, decimal FaturamentoMensal, RepresentanteLegalDto RepresentanteLegal);
