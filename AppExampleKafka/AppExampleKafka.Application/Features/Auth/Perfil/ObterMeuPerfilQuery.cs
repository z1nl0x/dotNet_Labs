using AppExampleKafka.Application.DTOs.Auth;
using MediatR;

namespace AppExampleKafka.Application.Features.Auth.Perfil;

public record ObterMeuPerfilQuery : IRequest<MeuPerfilDto>;
