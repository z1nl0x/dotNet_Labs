using AppExampleKafka.Application.DTOs.Contas;
using MediatR;

namespace AppExampleKafka.Application.Features.Contas.Create;

public record CriarContaCommand : IRequest<ContaBancariaDto>;
