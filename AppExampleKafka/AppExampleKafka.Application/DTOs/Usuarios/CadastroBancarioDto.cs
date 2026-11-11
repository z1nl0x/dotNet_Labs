using AppExampleKafka.Domain.Enums;

namespace AppExampleKafka.Application.DTOs.Usuarios;

public record CadastroBancarioDto(TipoPessoa TipoPessoa, string Documento, string Telefone, EnderecoDto Endereco, DadosPessoaFisicaDto? PessoaFisica = null, DadosPessoaJuridicaDto? PessoaJuridica = null);
