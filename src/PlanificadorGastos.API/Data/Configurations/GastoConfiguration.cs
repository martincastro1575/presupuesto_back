using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlanificadorGastos.API.Models.Entities;

namespace PlanificadorGastos.API.Data.Configurations;

public class GastoConfiguration : IEntityTypeConfiguration<Gasto>
{
    public void Configure(EntityTypeBuilder<Gasto> builder)
    {
        builder.ToTable("Gastos");

        builder.HasKey(g => g.Id);

        builder.Property(g => g.Monto)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(g => g.Fecha)
            .IsRequired();

        builder.Property(g => g.Descripcion)
            .HasMaxLength(500);

        builder.Property(g => g.CreatedAt)
            .IsRequired();

        // Relación con Usuario
        builder.HasOne(g => g.Usuario)
            .WithMany(u => u.Gastos)
            .HasForeignKey(g => g.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relación con Categoría
        builder.HasOne(g => g.Categoria)
            .WithMany(c => c.Gastos)
            .HasForeignKey(g => g.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);  // No eliminar categoría si tiene gastos

        // Índices para optimizar queries
        builder.HasIndex(g => new { g.UserId, g.Fecha });
        builder.HasIndex(g => g.CategoriaId);
        builder.HasIndex(g => g.Fecha);
    }
}
