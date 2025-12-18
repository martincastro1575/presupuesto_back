using PlanificadorGastos.API.Models.DTOs.Categorias;

namespace PlanificadorGastos.API.Services.Interfaces;

public interface ICategoriasService
{
    Task<IEnumerable<CategoriaResponse>> GetAllAsync();
    Task<CategoriaResponse?> GetByIdAsync(int id);
    Task<CategoriaResponse> CreateAsync(CreateCategoriaRequest request);
    Task<CategoriaResponse> UpdateAsync(int id, UpdateCategoriaRequest request);
    Task<bool> DeleteAsync(int id);
}
