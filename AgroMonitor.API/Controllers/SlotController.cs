using AgroMonitor.Application.DTOs;
using AgroMonitor.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgroMonitor.API.Controllers;

/// <summary>
/// Operações sobre slots (vagas de plantio). Lado "N" do relacionamento 1:N com espécies.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class SlotController(ISlotService slotService) : ControllerBase
{
    /// <summary>Lista todos os slots.</summary>
    /// <response code="200">Lista retornada.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<SlotResponse>), StatusCodes.Status200OK)]
    public IActionResult GetAll()
    {
        return Ok(slotService.GetAll());
    }

    /// <summary>Obtém um slot pelo Id.</summary>
    /// <param name="id">Identificador do slot.</param>
    /// <response code="200">Slot encontrado.</response>
    /// <response code="404">Não encontrado.</response>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(SlotResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(long id)
    {
        var slot = slotService.GetById(id);
        if (slot is null)
            return NotFound();

        return Ok(slot);
    }

    /// <summary>Cria um slot vinculado a uma espécie.</summary>
    /// <param name="request">Dados do slot.</param>
    /// <response code="201">Slot criado.</response>
    /// <response code="400">Modelo inválido ou regra de domínio violada.</response>
    /// <response code="404">Espécie informada não existe.</response>
    [HttpPost]
    [ProducesResponseType(typeof(SlotResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Create([FromBody] SlotRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var created = slotService.Create(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Atualiza um slot existente.</summary>
    /// <param name="id">Identificador do slot.</param>
    /// <param name="request">Novos dados.</param>
    /// <response code="200">Slot atualizado.</response>
    /// <response code="400">Modelo inválido ou regra de domínio violada.</response>
    /// <response code="404">Slot ou espécie não encontrados.</response>
    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(SlotResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Update(long id, [FromBody] SlotRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updated = slotService.Update(id, request);
        if (updated is null)
            return NotFound();

        return Ok(updated);
    }

    /// <summary>
    /// Remoção lógica do slot: marca como INACTIVE em vez de apagar,
    /// preservando o histórico (leituras/regas/alertas) que o referencia.
    /// </summary>
    /// <param name="id">Identificador do slot.</param>
    /// <response code="204">Slot desativado.</response>
    /// <response code="404">Não encontrado.</response>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(long id)
    {
        return slotService.Delete(id) ? NoContent() : NotFound();
    }
}
