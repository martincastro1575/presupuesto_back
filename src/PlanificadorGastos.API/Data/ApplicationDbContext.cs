using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PlanificadorGastos.API.Data.Configurations;
using PlanificadorGastos.API.Models.Entities;

namespace PlanificadorGastos.API.Data;

public class ApplicationDbContext : IdentityDbContext<Usuario, IdentityRole<int>, int>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Gasto> Gastos => Set<Gasto>();
    public DbSet<Presupuesto> Presupuestos => Set<Presupuesto>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplicar configuraciones
        modelBuilder.ApplyConfiguration(new UsuarioConfiguration());
        modelBuilder.ApplyConfiguration(new CategoriaConfiguration());
        modelBuilder.ApplyConfiguration(new GastoConfiguration());
        modelBuilder.ApplyConfiguration(new PresupuestoConfiguration());
        modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());

        // Renombrar tablas de Identity (opcional, para mantener consistencia)
        modelBuilder.Entity<Usuario>().ToTable("Usuarios");
        modelBuilder.Entity<IdentityRole<int>>().ToTable("Roles");
        modelBuilder.Entity<IdentityUserRole<int>>().ToTable("UsuarioRoles");
        modelBuilder.Entity<IdentityUserClaim<int>>().ToTable("UsuarioClaims");
        modelBuilder.Entity<IdentityUserLogin<int>>().ToTable("UsuarioLogins");
        modelBuilder.Entity<IdentityUserToken<int>>().ToTable("UsuarioTokens");
        modelBuilder.Entity<IdentityRoleClaim<int>>().ToTable("RoleClaims");

        // Seed de categorías predefinidas
        SeedCategorias(modelBuilder);
    }

    private static void SeedCategorias(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Categoria>().HasData(
            new Categoria { Id = 1, UserId = null, Nombre = "Alimentación", Icono = "pi-shopping-cart", Color = "#22c55e", EsPredefinida = true, Activa = true },
            new Categoria { Id = 2, UserId = null, Nombre = "Transporte", Icono = "pi-car", Color = "#3b82f6", EsPredefinida = true, Activa = true },
            new Categoria { Id = 3, UserId = null, Nombre = "Servicios", Icono = "pi-bolt", Color = "#f59e0b", EsPredefinida = true, Activa = true },
            new Categoria { Id = 4, UserId = null, Nombre = "Entretenimiento", Icono = "pi-ticket", Color = "#ec4899", EsPredefinida = true, Activa = true },
            new Categoria { Id = 5, UserId = null, Nombre = "Salud", Icono = "pi-heart", Color = "#ef4444", EsPredefinida = true, Activa = true },
            new Categoria { Id = 6, UserId = null, Nombre = "Educación", Icono = "pi-book", Color = "#8b5cf6", EsPredefinida = true, Activa = true },
            new Categoria { Id = 7, UserId = null, Nombre = "Hogar", Icono = "pi-home", Color = "#06b6d4", EsPredefinida = true, Activa = true },
            new Categoria { Id = 8, UserId = null, Nombre = "Ropa", Icono = "pi-tag", Color = "#f97316", EsPredefinida = true, Activa = true },
            new Categoria { Id = 9, UserId = null, Nombre = "Otros", Icono = "pi-ellipsis-h", Color = "#6b7280", EsPredefinida = true, Activa = true }
        );
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Actualizar UpdatedAt automáticamente
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is Gasto && e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            ((Gasto)entry.Entity).UpdatedAt = DateTime.UtcNow;
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
