namespace AppExampleKafka.Domain.Entities.Usuarios;

public record DadosPessoaJuridica(string RazaoSocial, string NomeFantasia, DateOnly DataConstituicao, string NaturezaJuridica, string Cnae, decimal FaturamentoMensal, RepresentanteLegal RepresentanteLegal)
{
    private DadosPessoaJuridica() : this("", "", default, "", "", 0m, null!) { }
}
