using AppTest.Application.DTOs.Common;
using AppTest.Application.DTOs.Produtos;
using AppTest.Application.Features.Produtos.Create;
using AppTest.Application.Features.Produtos.Delete;
using AppTest.Application.Features.Produtos.GetById;
using AppTest.Application.Features.Produtos.GetList;
using AppTest.Application.Features.Produtos.Update;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppTest.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,User")]
public class ProdutosController(ISender sender) : ControllerBase
{
    /// <summary>Lista os produtos com filtro opcional por nome e paginação.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProdutoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] GetProdutosQuery query, CancellationToken cancellationToken) =>
        Ok(await sender.Send(query, cancellationToken));

    /// <summary>Consulta um produto pelo id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProdutoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        Ok(await sender.Send(new GetProdutoByIdQuery(id), cancellationToken));

    /// <summary>Cria um novo produto vinculado a uma categoria.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProdutoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreateProdutoCommand command, CancellationToken cancellationToken)
    {
        var product = await sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    /// <summary>Atualiza um produto existente.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ProdutoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProdutoCommand command, CancellationToken cancellationToken) =>
        Ok(await sender.Send(command with { Id = id }, cancellationToken));

    /// <summary>Remove um produto.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteProdutoCommand(id), cancellationToken);
        return NoContent();
    }
}
