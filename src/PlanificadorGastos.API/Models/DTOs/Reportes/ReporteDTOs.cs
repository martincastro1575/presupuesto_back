namespace PlanificadorGastos.API.Models.DTOs.Reportes;

public record ResumenMensualResponse
{
    public int Anio { get; init; }
    public int Mes { get; init; }
    public decimal TotalGastado { get; init; }
    public int CantidadGastos { get; init; }
    public decimal PromedioGasto { get; init; }
    public decimal PromedioGastoDiario { get; init; }
    public decimal TotalMesAnterior { get; init; }
    public decimal DiferenciaMesAnterior { get; init; }
    public decimal PorcentajeCambio { get; init; }
    public GastoPorCategoriaResponse? CategoriaMayorGasto { get; init; }
}

public record GastoPorCategoriaResponse
{
    public int CategoriaId { get; init; }
    public string CategoriaNombre { get; init; } = string.Empty;
    public string CategoriaIcono { get; init; } = string.Empty;
    public string CategoriaColor { get; init; } = string.Empty;
    public decimal TotalMonto { get; init; }
    public int CantidadGastos { get; init; }
    public decimal Porcentaje { get; init; }
}

public record EvolucionMensualResponse
{
    public int Anio { get; init; }
    public int Mes { get; init; }
    public string MesNombre { get; init; } = string.Empty;
    public decimal TotalGastado { get; init; }
    public int CantidadGastos { get; init; }
}

public record ComparativoMensualResponse
{
    public ResumenMensualResponse MesActual { get; init; } = null!;
    public ResumenMensualResponse MesAnterior { get; init; } = null!;
    public List<ComparativoCategoriaResponse> ComparativoPorCategoria { get; init; } = [];
}

public record ComparativoCategoriaResponse
{
    public int CategoriaId { get; init; }
    public string CategoriaNombre { get; init; } = string.Empty;
    public string CategoriaColor { get; init; } = string.Empty;
    public decimal MontoMesActual { get; init; }
    public decimal MontoMesAnterior { get; init; }
    public decimal Diferencia { get; init; }
    public decimal PorcentajeCambio { get; init; }
}
