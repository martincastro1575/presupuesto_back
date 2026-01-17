using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlanificadorGastos.API.Models.Entities;

namespace PlanificadorGastos.API.Data.Configurations;

public class LimiteCategoriaConfiguration : IEntityTypeConfiguration<LimiteCategoria>
{
    public void Configure(EntityTypeBuilder<LimiteCategoria> builder)
    {
        builder.ToTable("LimitesCategorias");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.MontoLimite)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(l => l.Anio)
            .IsRequired();

        builder.Property(l => l.Mes)
            .IsRequired();

        builder.Property(l => l.CreatedAt)
            .IsRequired();

        // Relación con Usuario
        builder.HasOne(l => l.Usuario)
            .WithMany(u => u.LimitesCategorias)
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relación con Categoría (obligatoria)
        builder.HasOne(l => l.Categoria)
            .WithMany(c => c.LimitesCategorias)
            .HasForeignKey(l => l.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Constraint único: un límite por usuario/categoría/período
        builder.HasIndex(l => new { l.UserId, l.CategoriaId, l.Anio, l.Mes })
            .IsUnique()
            .HasDatabaseName("UQ_LimiteCategoria_Usuario_Categoria_Periodo");

        // Índices para optimizar queries
        builder.HasIndex(l => new { l.UserId, l.Anio, l.Mes });
        builder.HasIndex(l => l.CategoriaId);
    }
}
