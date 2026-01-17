using System.ComponentModel.DataAnnotations;

namespace PlanificadorGastos.API.Models.DTOs.LimitesCategorias;

/// <summary>
/// Response con información del límite y gasto acumulado
/// </summary>
public record LimiteCategoriaResponse
{
    public int Id { get; init; }
    public int CategoriaId { get; init; }
    public string CategoriaNombre { get; init; } = string.Empty;
    public string CategoriaColor { get; init; } = string.Empty;
    public string CategoriaIcono { get; init; } = string.Empty;
    public decimal MontoLimite { get; init; }
    public int Anio { get; init; }
    public int Mes { get; init; }
    public decimal MontoGastado { get; init; }
    public decimal MontoDisponible => MontoLimite - MontoGastado;
    public decimal PorcentajeConsumido => MontoLimite > 0
        ? Math.Round(MontoGastado / MontoLimite * 100, 2)
        : 0;
    public bool ExcedeLimite => MontoGastado > MontoLimite;
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// Request para crear un nuevo límite
/// </summary>
public record CreateLimiteCategoriaRequest
{
    [Required(ErrorMessage = "La categoría es requerida")]
    public int CategoriaId { get; init; }

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

/// <summary>
/// Request para actualizar el monto límite
/// </summary>
public record UpdateLimiteCategoriaRequest
{
    [Required(ErrorMessage = "El monto límite es requerido")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
    public decimal MontoLimite { get; init; }
}

/// <summary>
/// Request para crear límites en lote (múltiples categorías a la vez)
/// </summary>
public record CreateLimitesLoteRequest
{
    [Required(ErrorMessage = "El año es requerido")]
    [Range(2020, 2100, ErrorMessage = "El año debe estar entre 2020 y 2100")]
    public int Anio { get; init; }

    [Required(ErrorMessage = "El mes es requerido")]
    [Range(1, 12, ErrorMessage = "El mes debe estar entre 1 y 12")]
    public int Mes { get; init; }

    [Required(ErrorMessage = "Debe especificar al menos un límite")]
    [MinLength(1, ErrorMessage = "Debe especificar al menos un límite")]
    public List<LimiteItem> Limites { get; init; } = [];
}

public record LimiteItem
{
    [Required(ErrorMessage = "La categoría es requerida")]
    public int CategoriaId { get; init; }

    [Required(ErrorMessage = "El monto límite es requerido")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
    public decimal MontoLimite { get; init; }
}

/// <summary>
/// Resumen de límites de un período con totales
/// </summary>
public record ResumenLimitesPeriodoResponse
{
    public int Anio { get; init; }
    public int Mes { get; init; }
    public decimal TotalLimites { get; init; }
    public decimal TotalGastado { get; init; }
    public decimal TotalDisponible => TotalLimites - TotalGastado;
    public decimal PorcentajeGlobalConsumido => TotalLimites > 0
        ? Math.Round(TotalGastado / TotalLimites * 100, 2)
        : 0;
    public int CategoriasConLimite { get; init; }
    public int CategoriasExcedidas { get; init; }
    public List<LimiteCategoriaResponse> Limites { get; init; } = [];
}

/// <summary>
/// Histórico de límites de una categoría
/// </summary>
public record HistoricoLimiteCategoriaResponse
{
    public int CategoriaId { get; init; }
    public string CategoriaNombre { get; init; } = string.Empty;
    public List<LimiteCategoriaResponse> Historico { get; init; } = [];
}
