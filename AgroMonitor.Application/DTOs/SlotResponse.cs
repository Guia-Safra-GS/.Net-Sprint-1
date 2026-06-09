using AgroMonitor.Domain.Entities;

namespace AgroMonitor.Application.DTOs;

/// <summary>
/// DTO de resposta para slot (vaga de plantio).
/// </summary>
public record SlotResponse(
    long Id,
    long SpeciesId,
    string Position,
    DateOnly PlantedAt,
    string Status,
    DateTime CreatedAt)
{
    /// <summary>Mapeia <see cref="Slot"/> para DTO.</summary>
    public static SlotResponse FromDomain(Slot slot) =>
        new(
            slot.Id,
            slot.SpeciesId,
            slot.Position,
            slot.PlantedAt,
            slot.Status,
            slot.CreatedAt);
}
