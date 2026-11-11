using AppExampleKafka.Application.Common.Interfaces;
using AppExampleKafka.Domain.Repositories;
using AppExampleKafka.Domain.Entities.Bancario;
using AppExampleKafka.Application.DTOs.Contas;
using MediatR;
using AutoMapper;

namespace AppExampleKafka.Application.Features.Contas.Create;

public class CriarContaCommandHandler(IUnitOfWork uow, IUsuarioAtual usuario, IMapper mapper) : IRequestHandler<CriarContaCommand, ContaBancariaDto>
{
    public async Task<ContaBancariaDto> Handle(CriarContaCommand request, CancellationToken ct)
    {
        var conta = new ContaBancaria(usuario.Id);
        await uow.Contas.AddAsync(conta, ct);
        await uow.SaveChangesAsync(ct);
        return mapper.Map<ContaBancariaDto>(conta);
    }
}
