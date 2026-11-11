using AppExampleKafka.Application.Common.Interfaces;
using AppExampleKafka.Application.Common.Exceptions;
using AppExampleKafka.Domain.Repositories;
using AppExampleKafka.Application.DTOs.Contas;
using MediatR;
using AutoMapper;

namespace AppExampleKafka.Application.Features.Contas.GetById;

public class ObterContaQueryHandler(IUnitOfWork uow, IUsuarioAtual usuario, IMapper mapper) : IRequestHandler<ObterContaQuery, ContaBancariaDto>
{
    public async Task<ContaBancariaDto> Handle(ObterContaQuery request, CancellationToken ct)
    {
        var conta = await uow.Contas.GetAsync(request.Id, ct);
        if (conta is null || conta.UsuarioId != usuario.Id) throw new NotFoundException("Conta não encontrada.");
        return mapper.Map<ContaBancariaDto>(conta);
    }
}
