using AppExampleKafka.Application.Common.Interfaces;
using AppExampleKafka.Application.Common.Exceptions;
using AppExampleKafka.Domain.Repositories;
using AppExampleKafka.Application.DTOs.Auth;
using MediatR;
using AutoMapper;

namespace AppExampleKafka.Application.Features.Auth.Perfil;

public class ObterMeuPerfilQueryHandler(IUnitOfWork uow, IUsuarioAtual atual, IMapper mapper) : IRequestHandler<ObterMeuPerfilQuery, MeuPerfilDto>
{
    public async Task<MeuPerfilDto> Handle(ObterMeuPerfilQuery request, CancellationToken ct)
    {
        var usuario = await uow.Users.GetByIdAsync(atual.Id, ct) ?? throw new UnauthorizedException("Usuário não encontrado.");
        return mapper.Map<MeuPerfilDto>(usuario);
    }
}
