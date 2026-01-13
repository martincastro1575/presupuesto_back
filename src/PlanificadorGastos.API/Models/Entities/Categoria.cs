namespace PlanificadorGastos.API.Models.Entities;

public class Categoria
{
    public int Id { get; set; }

    public int? UserId { get; set; }  // Null para categorias predefinidas del sistema

    public string Nombre { get; set; } = string.Empty;

    public string Icono { get; set; } = "pi-tag";

    public string Color { get; set; } = "#6366f1";

    public TipoCategoria Tipo { get; set; } = TipoCategoria.Gasto;

    public bool EsPredefinida { get; set; } = false;

    public bool Activa { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Usuario? Usuario { get; set; }
    public virtual ICollection<Gasto> Gastos { get; set; } = [];
    public virtual ICollection<Ingreso> Ingresos { get; set; } = [];
    public virtual ICollection<Presupuesto> Presupuestos { get; set; } = [];
}
