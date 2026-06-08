using AgroMonitor.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgroMonitor.Infrastructure.Persistence.Configurations;

/// <summary>
/// Mapeamento EF da entidade <see cref="Species"/> para a tabela TB_CAD_SPECIES
/// do schema Oracle compartilhado.
/// </summary>
public sealed class SpeciesConfiguration : IEntityTypeConfiguration<Species>
{
    public void Configure(EntityTypeBuilder<Species> builder)
    {
        builder.ToTable("TB_CAD_SPECIES");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasColumnName("ID")
            .ValueGeneratedOnAdd();

        builder.Property(s => s.CommonName)
            .HasColumnName("COMMON_NAME")
            .HasMaxLength(80)
            .IsRequired();

        builder.HasIndex(s => s.CommonName)
            .IsUnique();

        builder.Property(s => s.ScientificName)
            .HasColumnName("SCIENTIFIC_NAME")
            .HasMaxLength(120);

        builder.Property(s => s.MinHumidity)
            .HasColumnName("MIN_HUMIDITY")
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(s => s.MaxHumidity)
            .HasColumnName("MAX_HUMIDITY")
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(s => s.FrostMinTemp)
            .HasColumnName("FROST_MIN_TEMP")
            .HasPrecision(4, 1);

        builder.Property(s => s.WateringMl)
            .HasColumnName("WATERING_ML")
            .IsRequired();

        builder.Property(s => s.CreatedAt)
            .HasColumnName("CREATED_AT");
    }
}
