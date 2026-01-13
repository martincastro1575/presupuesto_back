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

        builder.Property(c => c.Tipo)
            .IsRequired()
            .HasDefaultValue(TipoCategoria.Gasto);

        builder.Property(c => c.EsPredefinida)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(c => c.Activa)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(c => c.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        // Relacion con Usuario (opcional para categorias predefinidas)
        builder.HasOne(c => c.Usuario)
            .WithMany(u => u.Categorias)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indices
        builder.HasIndex(c => c.UserId);
        builder.HasIndex(c => new { c.UserId, c.Nombre });
        builder.HasIndex(c => c.Tipo);

        // Datos semilla - Categorias predefinidas de GASTOS
        builder.HasData(
            new Categoria { Id = 1, Nombre = "Alimentacion", Icono = "pi-shopping-cart", Color = "#22c55e", Tipo = TipoCategoria.Gasto, EsPredefinida = true },
            new Categoria { Id = 2, Nombre = "Transporte", Icono = "pi-car", Color = "#3b82f6", Tipo = TipoCategoria.Gasto, EsPredefinida = true },
            new Categoria { Id = 3, Nombre = "Servicios", Icono = "pi-bolt", Color = "#f59e0b", Tipo = TipoCategoria.Gasto, EsPredefinida = true },
            new Categoria { Id = 4, Nombre = "Entretenimiento", Icono = "pi-ticket", Color = "#ec4899", Tipo = TipoCategoria.Gasto, EsPredefinida = true },
            new Categoria { Id = 5, Nombre = "Salud", Icono = "pi-heart", Color = "#ef4444", Tipo = TipoCategoria.Gasto, EsPredefinida = true },
            new Categoria { Id = 6, Nombre = "Educacion", Icono = "pi-book", Color = "#8b5cf6", Tipo = TipoCategoria.Gasto, EsPredefinida = true },
            new Categoria { Id = 7, Nombre = "Hogar", Icono = "pi-home", Color = "#06b6d4", Tipo = TipoCategoria.Gasto, EsPredefinida = true },
            new Categoria { Id = 8, Nombre = "Ropa", Icono = "pi-tag", Color = "#f97316", Tipo = TipoCategoria.Gasto, EsPredefinida = true },
            new Categoria { Id = 9, Nombre = "Otros Gastos", Icono = "pi-ellipsis-h", Color = "#6b7280", Tipo = TipoCategoria.Gasto, EsPredefinida = true },
            // Categorias predefinidas de INGRESOS (IDs altos para evitar conflictos con categorias de usuario)
            new Categoria { Id = 100, Nombre = "Sueldo", Icono = "pi-briefcase", Color = "#22c55e", Tipo = TipoCategoria.Ingreso, EsPredefinida = true },
            new Categoria { Id = 101, Nombre = "Freelance", Icono = "pi-code", Color = "#3b82f6", Tipo = TipoCategoria.Ingreso, EsPredefinida = true },
            new Categoria { Id = 102, Nombre = "Inversiones", Icono = "pi-chart-line", Color = "#8b5cf6", Tipo = TipoCategoria.Ingreso, EsPredefinida = true },
            new Categoria { Id = 103, Nombre = "Alquiler", Icono = "pi-home", Color = "#f59e0b", Tipo = TipoCategoria.Ingreso, EsPredefinida = true },
            new Categoria { Id = 104, Nombre = "Regalo", Icono = "pi-gift", Color = "#ec4899", Tipo = TipoCategoria.Ingreso, EsPredefinida = true },
            new Categoria { Id = 105, Nombre = "Reembolso", Icono = "pi-replay", Color = "#06b6d4", Tipo = TipoCategoria.Ingreso, EsPredefinida = true },
            new Categoria { Id = 106, Nombre = "Otros Ingresos", Icono = "pi-dollar", Color = "#6b7280", Tipo = TipoCategoria.Ingreso, EsPredefinida = true }
        );
    }
}
