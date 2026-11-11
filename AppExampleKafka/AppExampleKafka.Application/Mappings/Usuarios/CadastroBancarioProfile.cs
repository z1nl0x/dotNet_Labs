using AppExampleKafka.Application.DTOs.Usuarios;
using AppExampleKafka.Domain.Entities.Usuarios;
using AutoMapper;

namespace AppExampleKafka.Application.Mappings.Usuarios;

public class CadastroBancarioProfile : Profile
{
    public CadastroBancarioProfile()
    {
        CreateMap<CadastroBancario, CadastroBancarioDto>();
        CreateMap<CadastroBancarioDto, CadastroBancario>();
        CreateMap<Endereco, EnderecoDto>();
        CreateMap<EnderecoDto, Endereco>();
        CreateMap<DadosPessoaFisica, DadosPessoaFisicaDto>();
        CreateMap<DadosPessoaFisicaDto, DadosPessoaFisica>();
        CreateMap<DadosPessoaJuridica, DadosPessoaJuridicaDto>();
        CreateMap<DadosPessoaJuridicaDto, DadosPessoaJuridica>();
        CreateMap<RepresentanteLegal, RepresentanteLegalDto>();
        CreateMap<RepresentanteLegalDto, RepresentanteLegal>();
    }
}
