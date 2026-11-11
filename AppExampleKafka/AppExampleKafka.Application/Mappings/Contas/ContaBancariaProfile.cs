using AppExampleKafka.Application.DTOs.Contas;
using AppExampleKafka.Domain.Entities.Bancario;
using AutoMapper;

namespace AppExampleKafka.Application.Mappings.Contas;

public class ContaBancariaProfile : Profile
{
    public ContaBancariaProfile()
    {
        CreateMap<ContaBancaria, ContaBancariaDto>();
    }
}
