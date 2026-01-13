using System.ComponentModel.DataAnnotations;
using PlanificadorGastos.API.Models.Entities;

namespace PlanificadorGastos.API.Models.DTOs.Categorias;

public record CategoriaResponse
{
    public int Id { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string Icono { get; init; } = string.Empty;
    public string Color { get; init; } = string.Empty;
    public TipoCategoria Tipo { get; init; }
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

    [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "El color debe ser un codigo hexadecimal valido (ej: #FF5733)")]
    public string Color { get; init; } = "#6366f1";

    [Required(ErrorMessage = "El tipo es requerido")]
    public TipoCategoria Tipo { get; init; } = TipoCategoria.Gasto;
}

public record UpdateCategoriaRequest
{
    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 50 caracteres")]
    public string Nombre { get; init; } = string.Empty;

    [StringLength(50)]
    public string Icono { get; init; } = string.Empty;

    [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "El color debe ser un codigo hexadecimal valido (ej: #FF5733)")]
    public string Color { get; init; } = string.Empty;

    public TipoCategoria Tipo { get; init; }

    public bool Activa { get; init; } = true;
}
