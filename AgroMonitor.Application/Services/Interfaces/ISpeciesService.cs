using AgroMonitor.Application.DTOs;

namespace AgroMonitor.Application.Services.Interfaces;

/// <summary>
/// Casos de uso de espécie.
/// </summary>
public interface ISpeciesService
{
    IReadOnlyList<SpeciesResponse> GetAll();

    SpeciesResponse? GetById(long id);

    SpeciesResponse Create(SpeciesRequest request);

    SpeciesResponse? Update(long id, SpeciesRequest request);

    bool Delete(long id);
}
