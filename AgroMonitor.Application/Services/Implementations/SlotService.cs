using AgroMonitor.Application.DTOs;
using AgroMonitor.Application.Repositories;
using AgroMonitor.Application.Services.Interfaces;
using AgroMonitor.Domain.Entities;

namespace AgroMonitor.Application.Services.Implementations;

/// <summary>
/// Orquestra os casos de uso de slot. Valida a existência da espécie (FK)
/// antes de criar/atualizar e aplica a remoção lógica.
/// </summary>
public sealed class SlotService(
    IRepository<Slot> slotRepository,
    IRepository<Species> speciesRepository) : ISlotService
{
    /// <inheritdoc />
    public IReadOnlyList<SlotResponse> GetAll()
    {
        return slotRepository
            .GetAll()
            .Select(SlotResponse.FromDomain)
            .ToList();
    }

    /// <inheritdoc />
    public SlotResponse? GetById(long id)
    {
        var slot = slotRepository.GetById(id);
        return slot is null ? null : SlotResponse.FromDomain(slot);
    }

    /// <inheritdoc />
    public SlotResponse Create(SlotRequest request)
    {
        EnsureSpeciesExists(request.SpeciesId);

        var slot = request.ToDomain();
        slotRepository.Add(slot);
        return SlotResponse.FromDomain(slot);
    }

    /// <inheritdoc />
    public SlotResponse? Update(long id, SlotRequest request)
    {
        var slot = slotRepository.GetById(id);
        if (slot is null)
            return null;

        EnsureSpeciesExists(request.SpeciesId);

        // Preserva o status atual; a alteração de status acontece pela remoção lógica.
        slot.Update(request.SpeciesId, request.Position, request.PlantedAt, slot.Status);

        slotRepository.Update(slot);
        return SlotResponse.FromDomain(slot);
    }

    /// <inheritdoc />
    public bool Delete(long id)
    {
        var slot = slotRepository.GetById(id);
        if (slot is null)
            return false;

        slot.Deactivate();
        slotRepository.Update(slot);
        return true;
    }

    private void EnsureSpeciesExists(long speciesId)
    {
        if (!speciesRepository.ExistsById(speciesId))
            throw new KeyNotFoundException($"Espécie {speciesId} não encontrada.");
    }
}
