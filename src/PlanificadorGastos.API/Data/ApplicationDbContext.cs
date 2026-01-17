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
    public DbSet<Ingreso> Ingresos => Set<Ingreso>();
    public DbSet<LimiteCategoria> LimitesCategorias => Set<LimiteCategoria>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplicar configuraciones
        modelBuilder.ApplyConfiguration(new UsuarioConfiguration());
        modelBuilder.ApplyConfiguration(new CategoriaConfiguration());
        modelBuilder.ApplyConfiguration(new GastoConfiguration());
        modelBuilder.ApplyConfiguration(new PresupuestoConfiguration());
        modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());
        modelBuilder.ApplyConfiguration(new IngresoConfiguration());
        modelBuilder.ApplyConfiguration(new LimiteCategoriaConfiguration());

        // Renombrar tablas de Identity (opcional, para mantener consistencia)
        modelBuilder.Entity<Usuario>().ToTable("Usuarios");
        modelBuilder.Entity<IdentityRole<int>>().ToTable("Roles");
        modelBuilder.Entity<IdentityUserRole<int>>().ToTable("UsuarioRoles");
        modelBuilder.Entity<IdentityUserClaim<int>>().ToTable("UsuarioClaims");
        modelBuilder.Entity<IdentityUserLogin<int>>().ToTable("UsuarioLogins");
        modelBuilder.Entity<IdentityUserToken<int>>().ToTable("UsuarioTokens");
        modelBuilder.Entity<IdentityRoleClaim<int>>().ToTable("RoleClaims");
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
