using AppTest.Application.DTOs.Categories;
using AppTest.Application.DTOs.Common;
using AppTest.Application.Features.Categorias.Create;
using AppTest.Application.Features.Categorias.Delete;
using AppTest.Application.Features.Categorias.GetById;
using AppTest.Application.Features.Categorias.GetList;
using AppTest.Application.Features.Categorias.Update;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppTest.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,User")]
public class CategoriasController(ISender sender) : ControllerBase
{
    /// <summary>Lista as categorias com filtro opcional por nome e paginação.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<CategoriaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] GetCategoriasQuery query, CancellationToken cancellationToken) =>
        Ok(await sender.Send(query, cancellationToken));

    /// <summary>Consulta uma categoria pelo id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CategoriaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        Ok(await sender.Send(new GetCategoriaByIdQuery(id), cancellationToken));

    /// <summary>Cria uma nova categoria.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CategoriaDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateCategoriaCommand command, CancellationToken cancellationToken)
    {
        var category = await sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
    }

    /// <summary>Atualiza uma categoria existente.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CategoriaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoriaCommand command, CancellationToken cancellationToken) =>
        Ok(await sender.Send(command with { Id = id }, cancellationToken));

    /// <summary>Remove uma categoria (falha se houver produtos vinculados).</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteCategoriaCommand(id), cancellationToken);
        return NoContent();
    }
}
