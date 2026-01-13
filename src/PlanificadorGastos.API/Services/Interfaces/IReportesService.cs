using PlanificadorGastos.API.Models.DTOs.Reportes;

namespace PlanificadorGastos.API.Services.Interfaces;

public interface IReportesService
{
    Task<ResumenMensualResponse> GetResumenMensualAsync(int? anio = null, int? mes = null);
    Task<IEnumerable<GastoPorCategoriaResponse>> GetGastosPorCategoriaAsync(int? anio = null, int? mes = null);
    Task<IEnumerable<IngresoPorCategoriaResponse>> GetIngresosPorCategoriaAsync(int? anio = null, int? mes = null);
    Task<IEnumerable<EvolucionMensualResponse>> GetEvolucionMensualAsync(int meses = 6);
    Task<ComparativoMensualResponse> GetComparativoMensualAsync();
}
