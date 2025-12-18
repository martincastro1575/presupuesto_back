namespace PlanificadorGastos.API.Models.Entities;

public class Presupuesto
{
    public int Id { get; set; }
    
    public int UserId { get; set; }
    
    public int? CategoriaId { get; set; }  // Null para presupuesto general
    
    public decimal MontoLimite { get; set; }
    
    public int Anio { get; set; }
    
    public int Mes { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Usuario Usuario { get; set; } = null!;
    public virtual Categoria? Categoria { get; set; }
}
