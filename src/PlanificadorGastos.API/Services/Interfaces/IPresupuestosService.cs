using PlanificadorGastos.API.Models.DTOs.Presupuestos;

namespace PlanificadorGastos.API.Services.Interfaces;

public interface IPresupuestosService
{
    Task<IEnumerable<PresupuestoResponse>> GetAllAsync();
    Task<IEnumerable<PresupuestoResponse>> GetByPeriodoAsync(int anio, int mes);
    Task<PresupuestoResponse?> GetByIdAsync(int id);
    Task<PresupuestoResponse> CreateOrUpdateAsync(CreatePresupuestoRequest request);
    Task<bool> DeleteAsync(int id);
}
