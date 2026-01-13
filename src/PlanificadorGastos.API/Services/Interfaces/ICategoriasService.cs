using PlanificadorGastos.API.Models.DTOs.Categorias;
using PlanificadorGastos.API.Models.Entities;

namespace PlanificadorGastos.API.Services.Interfaces;

public interface ICategoriasService
{
    Task<IEnumerable<CategoriaResponse>> GetAllAsync(TipoCategoria? tipo = null);
    Task<CategoriaResponse?> GetByIdAsync(int id);
    Task<CategoriaResponse> CreateAsync(CreateCategoriaRequest request);
    Task<CategoriaResponse> UpdateAsync(int id, UpdateCategoriaRequest request);
    Task<bool> DeleteAsync(int id);
}
