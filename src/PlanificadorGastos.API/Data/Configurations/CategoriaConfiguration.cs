using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlanificadorGastos.API.Models.Entities;

namespace PlanificadorGastos.API.Data.Configurations;

public class CategoriaConfiguration : IEntityTypeConfiguration<Categoria>
{
    public void Configure(EntityTypeBuilder<Categoria> builder)
    {
        builder.ToTable("Categorias");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Nombre)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.Icono)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("pi-tag");

        builder.Property(c => c.Color)
            .IsRequired()
            .HasMaxLength(7)
            .HasDefaultValue("#6366f1");

        builder.Property(c => c.EsPredefinida)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(c => c.Activa)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(c => c.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        // Relación con Usuario (opcional para categorías predefinidas)
        builder.HasOne(c => c.Usuario)
            .WithMany(u => u.Categorias)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex(c => c.UserId);
        builder.HasIndex(c => new { c.UserId, c.Nombre });
    }
}
