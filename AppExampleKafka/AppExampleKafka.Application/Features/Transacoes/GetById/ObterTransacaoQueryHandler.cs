using AppExampleKafka.Application.Common.Interfaces;
using AppExampleKafka.Application.Common.Exceptions;
using AppExampleKafka.Domain.Repositories;
using AppExampleKafka.Application.DTOs.Transacoes;
using MediatR;
using AutoMapper;

namespace AppExampleKafka.Application.Features.Transacoes.GetById;

public class ObterTransacaoQueryHandler(IUnitOfWork uow, IUsuarioAtual usuario, IMapper mapper) : IRequestHandler<ObterTransacaoQuery, TransacaoDto>
{
    public async Task<TransacaoDto> Handle(ObterTransacaoQuery request, CancellationToken ct) =>
        mapper.Map<TransacaoDto>(await uow.Transacoes.GetAsync(request.Id, usuario.Id, ct) ?? throw new NotFoundException("Transação não encontrada."));
}
