using AgroMonitor.Application.DTOs;
using AgroMonitor.Application.Repositories;
using AgroMonitor.Application.Services.Interfaces;

namespace AgroMonitor.Application.Services.Implementations;

/// <summary>
/// Orquestra os casos de uso de espécie.
/// </summary>
public sealed class SpeciesService(ISpeciesRepository speciesRepository) : ISpeciesService
{
    /// <inheritdoc />
    public IReadOnlyList<SpeciesResponse> GetAll()
    {
        return speciesRepository
            .GetAll()
            .Select(SpeciesResponse.FromDomain)
            .ToList();
    }

    /// <inheritdoc />
    public SpeciesResponse? GetById(long id)
    {
        var species = speciesRepository.GetById(id);
        return species is null ? null : SpeciesResponse.FromDomain(species);
    }

    /// <inheritdoc />
    public SpeciesResponse Create(SpeciesRequest request)
    {
        if (speciesRepository.ExistsByCommonName(request.CommonName))
            throw new InvalidOperationException($"Já existe uma espécie com o nome '{request.CommonName}'.");

        var species = request.ToDomain();
        speciesRepository.Add(species);
        return SpeciesResponse.FromDomain(species);
    }

    /// <inheritdoc />
    public SpeciesResponse? Update(long id, SpeciesRequest request)
    {
        var species = speciesRepository.GetById(id);
        if (species is null)
            return null;

        if (speciesRepository.ExistsByCommonName(request.CommonName, excludingId: id))
            throw new InvalidOperationException($"Já existe outra espécie com o nome '{request.CommonName}'.");

        species.Update(
            request.CommonName,
            request.ScientificName,
            request.MinHumidity,
            request.MaxHumidity,
            request.FrostMinTemp,
            request.WateringMl);

        speciesRepository.Update(species);
        return SpeciesResponse.FromDomain(species);
    }

    /// <inheritdoc />
    public bool Delete(long id)
    {
        return speciesRepository.Delete(id);
    }
}
