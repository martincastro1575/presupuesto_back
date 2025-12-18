using System.ComponentModel.DataAnnotations;
using PlanificadorGastos.API.Models.Common;

namespace PlanificadorGastos.API.Models.DTOs.Gastos;

public record GastoResponse
{
    public int Id { get; init; }
    public int CategoriaId { get; init; }
    public string CategoriaNombre { get; init; } = string.Empty;
    public string CategoriaIcono { get; init; } = string.Empty;
    public string CategoriaColor { get; init; } = string.Empty;
    public decimal Monto { get; init; }
    public DateOnly Fecha { get; init; }
    public string? Descripcion { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record CreateGastoRequest
{
    [Required(ErrorMessage = "La categoría es requerida")]
    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una categoría válida")]
    public int CategoriaId { get; init; }

    [Required(ErrorMessage = "El monto es requerido")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
    public decimal Monto { get; init; }

    [Required(ErrorMessage = "La fecha es requerida")]
    public DateOnly Fecha { get; init; }

    [StringLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres")]
    public string? Descripcion { get; init; }
}

public record UpdateGastoRequest
{
    [Required(ErrorMessage = "La categoría es requerida")]
    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una categoría válida")]
    public int CategoriaId { get; init; }

    [Required(ErrorMessage = "El monto es requerido")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
    public decimal Monto { get; init; }

    [Required(ErrorMessage = "La fecha es requerida")]
    public DateOnly Fecha { get; init; }

    [StringLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres")]
    public string? Descripcion { get; init; }
}

public class GastosFilterParams : PaginationParams
{
    public DateOnly? FechaDesde { get; set; }
    public DateOnly? FechaHasta { get; set; }
    public int? CategoriaId { get; set; }
    public string? OrdenarPor { get; set; } = "Fecha";
    public bool OrdenDesc { get; set; } = true;
}
