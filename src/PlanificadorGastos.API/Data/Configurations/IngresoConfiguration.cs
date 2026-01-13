using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlanificadorGastos.API.Models.Entities;

namespace PlanificadorGastos.API.Data.Configurations;

public class IngresoConfiguration : IEntityTypeConfiguration<Ingreso>
{
    public void Configure(EntityTypeBuilder<Ingreso> builder)
    {
        builder.ToTable("Ingresos");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Monto)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(i => i.Concepto)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(i => i.Fecha)
            .IsRequired();

        builder.Property(i => i.Descripcion)
            .HasMaxLength(500);

        builder.Property(i => i.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // Relacion con Usuario
        builder.HasOne(i => i.Usuario)
            .WithMany()
            .HasForeignKey(i => i.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relacion con Categoria
        builder.HasOne(i => i.Categoria)
            .WithMany(c => c.Ingresos)
            .HasForeignKey(i => i.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indices
        builder.HasIndex(i => new { i.UserId, i.Fecha });
        builder.HasIndex(i => i.CategoriaId);
    }
}
