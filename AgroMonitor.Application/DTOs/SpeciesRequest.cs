using System.ComponentModel.DataAnnotations;
using AgroMonitor.Domain.Entities;

namespace AgroMonitor.Application.DTOs;

/// <summary>
/// DTO de requisição para criação/atualização de espécie.
/// </summary>
public record SpeciesRequest(
    [param: Required(ErrorMessage = "O nome comum é obrigatório")]
    [param: StringLength(80, MinimumLength = 2, ErrorMessage = "O nome comum deve ter entre 2 e 80 caracteres")]
    string CommonName,

    [param: StringLength(120, ErrorMessage = "O nome científico deve ter no máximo 120 caracteres")]
    string? ScientificName,

    [param: Range(0, 100, ErrorMessage = "A umidade mínima deve estar entre 0 e 100")]
    decimal MinHumidity,

    [param: Range(0, 100, ErrorMessage = "A umidade máxima deve estar entre 0 e 100")]
    decimal MaxHumidity,

    decimal? FrostMinTemp,

    [param: Range(1, int.MaxValue, ErrorMessage = "O volume de rega deve ser maior que zero")]
    int WateringMl)
{
    /// <summary>Constroi a entidade de domínio <see cref="Species"/>.</summary>
    public Species ToDomain() =>
        new(CommonName, ScientificName, MinHumidity, MaxHumidity, FrostMinTemp, WateringMl);
}
