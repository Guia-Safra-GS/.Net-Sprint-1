using AgroMonitor.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgroMonitor.Infrastructure.Persistence.Configurations;

/// <summary>
/// Mapeamento EF da entidade <see cref="Slot"/> para a tabela TB_CAD_SLOT.
/// Configura o relacionamento 1:N com <see cref="Species"/> (FK SPECIES_ID),
/// com exclusão RESTRITA — não se apaga uma espécie que tenha slots.
/// </summary>
public sealed class SlotConfiguration : IEntityTypeConfiguration<Slot>
{
    public void Configure(EntityTypeBuilder<Slot> builder)
    {
        builder.ToTable("TB_CAD_SLOT");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasColumnName("ID")
            .ValueGeneratedOnAdd();

        builder.Property(s => s.SpeciesId)
            .HasColumnName("SPECIES_ID")
            .IsRequired();

        builder.Property(s => s.Position)
            .HasColumnName("POSITION")
            .HasMaxLength(20)
            .IsRequired();

        // O Oracle DATE não aceita DateOnly diretamente; converte para DateTime
        // (meia-noite) na escrita e de volta para DateOnly na leitura.
        builder.Property(s => s.PlantedAt)
            .HasColumnName("PLANTED_AT")
            .HasConversion(
                d => d.ToDateTime(TimeOnly.MinValue),
                dt => DateOnly.FromDateTime(dt))
            .HasColumnType("DATE")
            .IsRequired();

        builder.Property(s => s.Status)
            .HasColumnName("STATUS")
            .HasMaxLength(15)
            .IsRequired();

        builder.Property(s => s.CreatedAt)
            .HasColumnName("CREATED_AT");

        builder.HasOne(s => s.Species)
            .WithMany(sp => sp.Slots)
            .HasForeignKey(s => s.SpeciesId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
