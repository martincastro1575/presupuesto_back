using Microsoft.AspNetCore.Identity;

namespace PlanificadorGastos.API.Models.Entities;

public class Usuario : IdentityUser<int>
{
    public string Nombre { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? LastLogin { get; set; }
    
    // Navigation properties
    public virtual ICollection<Categoria> Categorias { get; set; } = [];
    public virtual ICollection<Gasto> Gastos { get; set; } = [];
    public virtual ICollection<Presupuesto> Presupuestos { get; set; } = [];
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}
