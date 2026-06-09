using AgroMonitor.Domain.Entities;

namespace AgroMonitor.Application.Repositories;

/// <summary>
/// Contrato de persistência específico de <see cref="Species"/>,
/// além do CRUD genérico de <see cref="IRepository{T}"/>.
/// </summary>
public interface ISpeciesRepository : IRepository<Species>
{
    /// <summary>
    /// Indica se já existe uma espécie com o mesmo nome comum.
    /// <paramref name="excludingId"/> permite ignorar o próprio registro numa atualização.
    /// </summary>
    bool ExistsByCommonName(string commonName, long? excludingId = null);
}
