using AgroMonitor.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgroMonitor.Infrastructure.Persistence;

/// <summary>
/// Contexto EF Core do domínio de Cadastro (C#).
/// Dono da escrita de TB_CAD_SPECIES e TB_CAD_SLOT no schema Oracle compartilhado.
/// As entidades são mapeadas por IEntityTypeConfiguration aplicadas via assembly.
/// </summary>
public class AgroMonitorContext(DbContextOptions<AgroMonitorContext> options) : DbContext(options)
{
    public DbSet<Species> Species { get; set; }

    public DbSet<Slot> Slots { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AgroMonitorContext).Assembly);
    }
}
