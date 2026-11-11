using AppExampleKafka.Application.DTOs.Transacoes;
using AppExampleKafka.Domain.Entities.Bancario;
using AutoMapper;

namespace AppExampleKafka.Application.Mappings.Transacoes;

public class TransacaoProfile : Profile
{
    public TransacaoProfile()
    {
        CreateMap<Transacao, TransacaoDto>();
    }
}
