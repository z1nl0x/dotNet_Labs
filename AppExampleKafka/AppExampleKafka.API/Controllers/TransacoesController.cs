using AppExampleKafka.Application.Features.Transacoes.Create;
using AppExampleKafka.Application.Features.Transacoes.GetById;
using AppExampleKafka.Application.Features.Transacoes.GetList;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppExampleKafka.API.Controllers;

[ApiController]
[Authorize]
[Route("api/transacoes")]
public class TransacoesController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarTransacaoCommand command, CancellationToken ct)
    {
        var transacao = await sender.Send(command, ct);
        return CreatedAtAction(nameof(Obter), new { id = transacao.Id }, transacao);
    }
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Obter(Guid id, CancellationToken ct) => Ok(await sender.Send(new ObterTransacaoQuery(id), ct));
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default) =>
        Ok(await sender.Send(new ListarTransacoesQuery(pageNumber, pageSize), ct));
}
