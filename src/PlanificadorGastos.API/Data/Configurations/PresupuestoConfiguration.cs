using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlanificadorGastos.API.Models.Entities;

namespace PlanificadorGastos.API.Data.Configurations;

public class PresupuestoConfiguration : IEntityTypeConfiguration<Presupuesto>
{
    public void Configure(EntityTypeBuilder<Presupuesto> builder)
    {
        builder.ToTable("Presupuestos");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.MontoLimite)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(p => p.Anio)
            .IsRequired();

        builder.Property(p => p.Mes)
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        // Relación con Usuario
        builder.HasOne(p => p.Usuario)
            .WithMany(u => u.Presupuestos)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relación con Categoría (opcional)
        builder.HasOne(p => p.Categoria)
            .WithMany(c => c.Presupuestos)
            .HasForeignKey(p => p.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Constraint único: un presupuesto por usuario/categoría/período
        builder.HasIndex(p => new { p.UserId, p.CategoriaId, p.Anio, p.Mes })
            .IsUnique()
            .HasDatabaseName("UQ_Presupuesto_Usuario_Categoria_Periodo");
    }
}
