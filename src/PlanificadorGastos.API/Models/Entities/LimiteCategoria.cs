namespace PlanificadorGastos.API.Models.Entities;

public class LimiteCategoria
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int CategoriaId { get; set; }

    public decimal MontoLimite { get; set; }

    public int Anio { get; set; }

    public int Mes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual Usuario Usuario { get; set; } = null!;
    public virtual Categoria Categoria { get; set; } = null!;
}
