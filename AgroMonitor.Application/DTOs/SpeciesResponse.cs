using AgroMonitor.Domain.Entities;

namespace AgroMonitor.Application.DTOs;

/// <summary>
/// DTO de resposta para espécie.
/// </summary>
public record SpeciesResponse(
    long Id,
    string CommonName,
    string? ScientificName,
    decimal MinHumidity,
    decimal MaxHumidity,
    decimal? FrostMinTemp,
    int WateringMl,
    DateTime CreatedAt)
{
    /// <summary>Mapeia <see cref="Species"/> para DTO.</summary>
    public static SpeciesResponse FromDomain(Species species) =>
        new(
            species.Id,
            species.CommonName,
            species.ScientificName,
            species.MinHumidity,
            species.MaxHumidity,
            species.FrostMinTemp,
            species.WateringMl,
            species.CreatedAt);
}
