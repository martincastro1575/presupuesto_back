using PlanificadorGastos.API.Models.Common;
using PlanificadorGastos.API.Models.DTOs.Gastos;

namespace PlanificadorGastos.API.Services.Interfaces;

public interface IGastosService
{
    Task<PaginatedResult<GastoResponse>> GetAllAsync(GastosFilterParams filterParams);
    Task<GastoResponse?> GetByIdAsync(int id);
    Task<GastoResponse> CreateAsync(CreateGastoRequest request);
    Task<GastoResponse> UpdateAsync(int id, UpdateGastoRequest request);
    Task<bool> DeleteAsync(int id);
}
