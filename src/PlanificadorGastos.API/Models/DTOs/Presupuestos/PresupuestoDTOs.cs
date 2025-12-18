using System.ComponentModel.DataAnnotations;

namespace PlanificadorGastos.API.Models.DTOs.Presupuestos;

public record PresupuestoResponse
{
    public int Id { get; init; }
    public int? CategoriaId { get; init; }
    public string? CategoriaNombre { get; init; }
    public string? CategoriaColor { get; init; }
    public decimal MontoLimite { get; init; }
    public int Anio { get; init; }
    public int Mes { get; init; }
    public decimal MontoGastado { get; init; }
    public decimal MontoDisponible => MontoLimite - MontoGastado;
    public decimal PorcentajeConsumido => MontoLimite > 0 ? Math.Round(MontoGastado / MontoLimite * 100, 2) : 0;
}

public record CreatePresupuestoRequest
{
    public int? CategoriaId { get; init; }  // Null para presupuesto general

    [Required(ErrorMessage = "El monto límite es requerido")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
    public decimal MontoLimite { get; init; }

    [Required(ErrorMessage = "El año es requerido")]
    [Range(2020, 2100, ErrorMessage = "El año debe estar entre 2020 y 2100")]
    public int Anio { get; init; }

    [Required(ErrorMessage = "El mes es requerido")]
    [Range(1, 12, ErrorMessage = "El mes debe estar entre 1 y 12")]
    public int Mes { get; init; }
}

public record UpdatePresupuestoRequest
{
    [Required(ErrorMessage = "El monto límite es requerido")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
    public decimal MontoLimite { get; init; }
}
