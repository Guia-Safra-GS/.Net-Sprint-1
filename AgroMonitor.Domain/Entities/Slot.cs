using AgroMonitor.Domain.Common;
using AgroMonitor.Domain.Exceptions;

namespace AgroMonitor.Domain.Entities;

/// <summary>
/// Vaga de plantio monitorada. É o lado "N" do relacionamento 1:N com
/// <see cref="Species"/> (cada slot tem uma espécie). Mapeia TB_CAD_SLOT.
/// A remoção é lógica: em vez de apagar, o slot passa a INACTIVE, preservando
/// o histórico de leituras/regas/alertas que o referenciam por FK.
/// </summary>
public sealed class Slot : BaseEntity
{
    public const string StatusActive = "ACTIVE";
    public const string StatusInactive = "INACTIVE";
    public const string StatusHarvested = "HARVESTED";

    private static readonly string[] AllowedStatuses =
        [StatusActive, StatusInactive, StatusHarvested];

    public long SpeciesId { get; private set; }

    public Species? Species { get; private set; }

    public string Position { get; private set; } = string.Empty;

    public DateOnly PlantedAt { get; private set; }

    public string Status { get; private set; } = StatusActive;

    public Slot(long speciesId, string position, DateOnly plantedAt)
    {
        SetSpecies(speciesId);
        SetPosition(position);
        PlantedAt = plantedAt;
        Status = StatusActive;
    }

    // Construtor sem parâmetros exigido pelo EF Core.
    private Slot()
    {
    }

    /// <summary>Atualiza os dados editáveis do slot, revalidando o domínio.</summary>
    public void Update(long speciesId, string position, DateOnly plantedAt, string status)
    {
        SetSpecies(speciesId);
        SetPosition(position);
        PlantedAt = plantedAt;
        SetStatus(status);
    }

    /// <summary>Remoção lógica: marca o slot como inativo.</summary>
    public void Deactivate() => Status = StatusInactive;

    private void SetSpecies(long speciesId)
    {
        if (speciesId <= 0)
            throw new DomainException("O slot precisa estar associado a uma espécie válida.");

        SpeciesId = speciesId;
    }

    private void SetPosition(string position)
    {
        if (string.IsNullOrWhiteSpace(position))
            throw new DomainException("A posição do slot é obrigatória.");

        Position = position.Trim();
    }

    private void SetStatus(string status)
    {
        if (string.IsNullOrWhiteSpace(status) || !AllowedStatuses.Contains(status))
            throw new DomainException(
                $"Status inválido. Valores aceitos: {string.Join(", ", AllowedStatuses)}.");

        Status = status;
    }
}
