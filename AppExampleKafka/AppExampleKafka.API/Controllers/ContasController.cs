using AppExampleKafka.Application.Features.Contas.Create;
using AppExampleKafka.Application.Features.Contas.GetById;
using AppExampleKafka.Application.Features.Contas.GetList;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppExampleKafka.API.Controllers;

[ApiController]
[Authorize]
[Route("api/contas")]
public class ContasController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Criar(CancellationToken ct)
    {
        var conta = await sender.Send(new CriarContaCommand(), ct);
        return CreatedAtAction(nameof(Obter), new { id = conta.Id }, conta);
    }
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Obter(Guid id, CancellationToken ct) => Ok(await sender.Send(new ObterContaQuery(id), ct));
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default) =>
        Ok(await sender.Send(new ListarContasQuery(pageNumber, pageSize), ct));
}
