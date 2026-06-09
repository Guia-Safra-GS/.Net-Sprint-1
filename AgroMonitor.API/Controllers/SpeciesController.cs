using AgroMonitor.Application.DTOs;
using AgroMonitor.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgroMonitor.API.Controllers;

/// <summary>
/// Operações sobre espécies de plantas. Lado "1" do relacionamento 1:N com slots.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class SpeciesController(ISpeciesService speciesService) : ControllerBase
{
    /// <summary>Lista todas as espécies.</summary>
    /// <response code="200">Lista retornada.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<SpeciesResponse>), StatusCodes.Status200OK)]
    public IActionResult GetAll()
    {
        return Ok(speciesService.GetAll());
    }

    /// <summary>Obtém uma espécie pelo Id.</summary>
    /// <param name="id">Identificador da espécie.</param>
    /// <response code="200">Espécie encontrada.</response>
    /// <response code="404">Não encontrada.</response>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(SpeciesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(long id)
    {
        var species = speciesService.GetById(id);
        if (species is null)
            return NotFound();

        return Ok(species);
    }

    /// <summary>Cria uma espécie.</summary>
    /// <param name="request">Dados da espécie.</param>
    /// <response code="201">Espécie criada.</response>
    /// <response code="400">Modelo inválido ou regra de domínio violada.</response>
    /// <response code="409">Já existe espécie com o mesmo nome.</response>
    [HttpPost]
    [ProducesResponseType(typeof(SpeciesResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult Create([FromBody] SpeciesRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var created = speciesService.Create(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Atualiza uma espécie existente.</summary>
    /// <param name="id">Identificador da espécie.</param>
    /// <param name="request">Novos dados.</param>
    /// <response code="200">Espécie atualizada.</response>
    /// <response code="400">Modelo inválido ou regra de domínio violada.</response>
    /// <response code="404">Não encontrada.</response>
    /// <response code="409">Já existe outra espécie com o mesmo nome.</response>
    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(SpeciesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult Update(long id, [FromBody] SpeciesRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updated = speciesService.Update(id, request);
        if (updated is null)
            return NotFound();

        return Ok(updated);
    }

    /// <summary>Remove uma espécie pelo Id.</summary>
    /// <param name="id">Identificador da espécie.</param>
    /// <response code="204">Removida.</response>
    /// <response code="404">Não encontrada.</response>
    /// <response code="409">Há slots vinculados a esta espécie (restrição de chave estrangeira).</response>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult Delete(long id)
    {
        return speciesService.Delete(id) ? NoContent() : NotFound();
    }
}
