using PlanificadorGastos.API.Models.DTOs.Ingresos;

namespace PlanificadorGastos.API.Services.Interfaces;

public interface IIngresosService
{
    Task<IEnumerable<IngresoResponse>> GetAllAsync();
    Task<IEnumerable<IngresoResponse>> GetByPeriodoAsync(int anio, int mes);
    Task<IngresoResponse?> GetByIdAsync(int id);
    Task<IngresoResponse> CreateAsync(CreateIngresoRequest request);
    Task<IngresoResponse?> UpdateAsync(int id, UpdateIngresoRequest request);
    Task<bool> DeleteAsync(int id);
    Task<decimal> GetTotalByPeriodoAsync(int anio, int mes);
}
