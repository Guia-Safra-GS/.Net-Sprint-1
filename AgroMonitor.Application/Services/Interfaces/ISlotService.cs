using AgroMonitor.Application.DTOs;

namespace AgroMonitor.Application.Services.Interfaces;

/// <summary>
/// Casos de uso de slot (vaga de plantio).
/// </summary>
public interface ISlotService
{
    IReadOnlyList<SlotResponse> GetAll();

    SlotResponse? GetById(long id);

    SlotResponse Create(SlotRequest request);

    SlotResponse? Update(long id, SlotRequest request);

    /// <summary>Remoção lógica: marca o slot como inativo. Retorna false se não existir.</summary>
    bool Delete(long id);
}
