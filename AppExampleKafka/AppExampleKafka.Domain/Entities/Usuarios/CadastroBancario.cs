using AppExampleKafka.Domain.Enums;
namespace AppExampleKafka.Domain.Entities.Usuarios;

public record CadastroBancario(TipoPessoa TipoPessoa, string Documento, string Telefone, Endereco Endereco, DadosPessoaFisica? PessoaFisica = null, DadosPessoaJuridica? PessoaJuridica = null)
{
    private CadastroBancario() : this(default, "", "", null!) { }
}
