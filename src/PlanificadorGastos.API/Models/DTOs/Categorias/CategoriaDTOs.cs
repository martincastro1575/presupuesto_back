using System.ComponentModel.DataAnnotations;

namespace PlanificadorGastos.API.Models.DTOs.Categorias;

public record CategoriaResponse
{
    public int Id { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string Icono { get; init; } = string.Empty;
    public string Color { get; init; } = string.Empty;
    public bool EsPredefinida { get; init; }
    public bool Activa { get; init; }
}

public record CreateCategoriaRequest
{
    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 50 caracteres")]
    public string Nombre { get; init; } = string.Empty;

    [StringLength(50)]
    public string Icono { get; init; } = "pi-tag";

    [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "El color debe ser un código hexadecimal válido (ej: #FF5733)")]
    public string Color { get; init; } = "#6366f1";
}

public record UpdateCategoriaRequest
{
    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 50 caracteres")]
    public string Nombre { get; init; } = string.Empty;

    [StringLength(50)]
    public string Icono { get; init; } = string.Empty;

    [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "El color debe ser un código hexadecimal válido (ej: #FF5733)")]
    public string Color { get; init; } = string.Empty;

    public bool Activa { get; init; } = true;
}
