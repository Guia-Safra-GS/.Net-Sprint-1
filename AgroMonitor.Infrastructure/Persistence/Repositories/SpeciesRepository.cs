using AgroMonitor.Application.Repositories;
using AgroMonitor.Domain.Entities;

namespace AgroMonitor.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repositório de <see cref="Species"/>: herda o CRUD genérico e acrescenta
/// a verificação de unicidade do nome comum.
/// </summary>
public sealed class SpeciesRepository(AgroMonitorContext context)
    : Repository<Species>(context), ISpeciesRepository
{
    public bool ExistsByCommonName(string commonName, long? excludingId = null)
    {
        if (string.IsNullOrWhiteSpace(commonName))
            return false;

        var normalized = commonName.Trim().ToLower();

        return Context.Set<Species>().Any(s =>
            s.CommonName.ToLower() == normalized &&
            (excludingId == null || s.Id != excludingId));
    }
}
