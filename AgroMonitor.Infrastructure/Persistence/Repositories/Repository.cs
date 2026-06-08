using AgroMonitor.Application.Repositories;
using AgroMonitor.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace AgroMonitor.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementação genérica de <see cref="IRepository{T}"/> sobre o EF Core.
/// Atende ao CRUD de qualquer agregado que derive de <see cref="BaseEntity"/>.
/// </summary>
/// <typeparam name="T">Tipo da entidade de domínio.</typeparam>
public class Repository<T>(AgroMonitorContext context) : IRepository<T> where T : BaseEntity
{
    protected AgroMonitorContext Context { get; } = context;

    private readonly DbSet<T> _set = context.Set<T>();

    public IReadOnlyList<T> GetAll()
    {
        return _set
            .OrderBy(e => e.Id)
            .ToList();
    }

    public T? GetById(long id)
    {
        return _set.Find(id);
    }

    public T Add(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        _set.Add(entity);
        Context.SaveChanges();

        return entity;
    }

    public T Update(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        _set.Update(entity);
        Context.SaveChanges();

        return entity;
    }

    public bool Delete(long id)
    {
        var entity = GetById(id);
        if (entity is null)
            return false;

        _set.Remove(entity);
        Context.SaveChanges();

        return true;
    }

    public bool ExistsById(long id)
    {
        return _set.Any(e => e.Id == id);
    }
}
