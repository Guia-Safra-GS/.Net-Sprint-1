using System.ComponentModel.DataAnnotations;
using AgroMonitor.Domain.Entities;

namespace AgroMonitor.Application.DTOs;

/// <summary>
/// DTO de requisição para criação/atualização de slot (vaga de plantio).
/// </summary>
public record SlotRequest(
    [param: Range(1, long.MaxValue, ErrorMessage = "O slot deve referenciar uma espécie válida")]
    long SpeciesId,

    [param: Required(ErrorMessage = "A posição é obrigatória")]
    [param: StringLength(20, MinimumLength = 1, ErrorMessage = "A posição deve ter no máximo 20 caracteres")]
    string Position,

    [param: Required(ErrorMessage = "A data de plantio é obrigatória")]
    DateOnly PlantedAt)
{
    /// <summary>Constroi a entidade de domínio <see cref="Slot"/>.</summary>
    public Slot ToDomain() => new(SpeciesId, Position, PlantedAt);
}
