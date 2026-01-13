using System.ComponentModel.DataAnnotations;

namespace PlanificadorGastos.API.Models.DTOs.Ingresos;

public record IngresoResponse
{
    public int Id { get; init; }
    public int CategoriaId { get; init; }
    public string CategoriaNombre { get; init; } = string.Empty;
    public string CategoriaIcono { get; init; } = string.Empty;
    public string CategoriaColor { get; init; } = string.Empty;
    public decimal Monto { get; init; }
    public string Concepto { get; init; } = string.Empty;
    public DateTime Fecha { get; init; }
    public string? Descripcion { get; init; }
}

public record CreateIngresoRequest
{
    [Required(ErrorMessage = "La categoria es requerida")]
    public int CategoriaId { get; init; }

    [Required(ErrorMessage = "El monto es requerido")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
    public decimal Monto { get; init; }

    [Required(ErrorMessage = "El concepto es requerido")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El concepto debe tener entre 2 y 100 caracteres")]
    public string Concepto { get; init; } = string.Empty;

    [Required(ErrorMessage = "La fecha es requerida")]
    public DateTime Fecha { get; init; }

    [StringLength(500)]
    public string? Descripcion { get; init; }
}

public record UpdateIngresoRequest
{
    [Required(ErrorMessage = "La categoria es requerida")]
    public int CategoriaId { get; init; }

    [Required(ErrorMessage = "El monto es requerido")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
    public decimal Monto { get; init; }

    [Required(ErrorMessage = "El concepto es requerido")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El concepto debe tener entre 2 y 100 caracteres")]
    public string Concepto { get; init; } = string.Empty;

    [Required(ErrorMessage = "La fecha es requerida")]
    public DateTime Fecha { get; init; }

    [StringLength(500)]
    public string? Descripcion { get; init; }
}
