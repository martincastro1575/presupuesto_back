namespace PlanificadorGastos.API.Models.Entities;

public class Gasto
{
    public int Id { get; set; }
    
    public int UserId { get; set; }
    
    public int CategoriaId { get; set; }
    
    public decimal Monto { get; set; }
    
    public DateOnly Fecha { get; set; }
    
    public string? Descripcion { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual Usuario Usuario { get; set; } = null!;
    public virtual Categoria Categoria { get; set; } = null!;
}
