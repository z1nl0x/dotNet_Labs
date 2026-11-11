using AppExampleKafka.Application.DTOs.Contas;
using MediatR;

namespace AppExampleKafka.Application.Features.Contas.GetById;

public record ObterContaQuery(Guid Id) : IRequest<ContaBancariaDto>;
