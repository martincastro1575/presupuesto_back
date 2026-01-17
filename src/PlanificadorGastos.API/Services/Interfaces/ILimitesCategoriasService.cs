using PlanificadorGastos.API.Models.DTOs.LimitesCategorias;

namespace PlanificadorGastos.API.Services.Interfaces;

public interface ILimitesCategoriasService
{
    /// <summary>
    /// Obtener todos los límites del usuario
    /// </summary>
    Task<IEnumerable<LimiteCategoriaResponse>> GetAllAsync();

    /// <summary>
    /// Obtener límites de un período específico con resumen
    /// </summary>
    Task<ResumenLimitesPeriodoResponse> GetByPeriodoAsync(int anio, int mes);

    /// <summary>
    /// Obtener un límite por ID
    /// </summary>
    Task<LimiteCategoriaResponse?> GetByIdAsync(int id);

    /// <summary>
    /// Obtener límite de una categoría en un período específico
    /// </summary>
    Task<LimiteCategoriaResponse?> GetByCategoriaYPeriodoAsync(int categoriaId, int anio, int mes);

    /// <summary>
    /// Obtener histórico de límites de una categoría
    /// </summary>
    Task<HistoricoLimiteCategoriaResponse> GetHistoricoByCategoriaAsync(int categoriaId);

    /// <summary>
    /// Crear o actualizar un límite (upsert)
    /// </summary>
    Task<LimiteCategoriaResponse> CreateOrUpdateAsync(CreateLimiteCategoriaRequest request);

    /// <summary>
    /// Crear múltiples límites en lote para un período
    /// </summary>
    Task<ResumenLimitesPeriodoResponse> CreateLoteAsync(CreateLimitesLoteRequest request);

    /// <summary>
    /// Actualizar el monto de un límite existente
    /// </summary>
    Task<LimiteCategoriaResponse?> UpdateAsync(int id, UpdateLimiteCategoriaRequest request);

    /// <summary>
    /// Eliminar un límite
    /// </summary>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Copiar límites de un período a otro
    /// </summary>
    Task<ResumenLimitesPeriodoResponse> CopiarPeriodoAsync(int anioOrigen, int mesOrigen, int anioDestino, int mesDestino);
}
