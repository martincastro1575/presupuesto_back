using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlanificadorGastos.API.Models.Common;
using PlanificadorGastos.API.Models.DTOs.LimitesCategorias;
using PlanificadorGastos.API.Services.Interfaces;

namespace PlanificadorGastos.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LimitesCategoriasController : ControllerBase
{
    private readonly ILimitesCategoriasService _limitesService;

    public LimitesCategoriasController(ILimitesCategoriasService limitesService)
    {
        _limitesService = limitesService;
    }

    /// <summary>
    /// Obtener todos los límites del usuario
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<LimiteCategoriaResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<LimiteCategoriaResponse>>>> GetAll()
    {
        var limites = await _limitesService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<LimiteCategoriaResponse>>.Ok(limites));
    }

    /// <summary>
    /// Obtener límites de un período específico con resumen
    /// </summary>
    [HttpGet("{anio:int}/{mes:int}")]
    [ProducesResponseType(typeof(ApiResponse<ResumenLimitesPeriodoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ResumenLimitesPeriodoResponse>>> GetByPeriodo(int anio, int mes)
    {
        if (mes < 1 || mes > 12)
        {
            return BadRequest(ApiResponse.Fail("El mes debe estar entre 1 y 12"));
        }

        var resumen = await _limitesService.GetByPeriodoAsync(anio, mes);
        return Ok(ApiResponse<ResumenLimitesPeriodoResponse>.Ok(resumen));
    }

    /// <summary>
    /// Obtener un límite por ID
    /// </summary>
    [HttpGet("detalle/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<LimiteCategoriaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<LimiteCategoriaResponse>>> GetById(int id)
    {
        var limite = await _limitesService.GetByIdAsync(id);

        if (limite == null)
        {
            return NotFound(ApiResponse.Fail("Límite no encontrado"));
        }

        return Ok(ApiResponse<LimiteCategoriaResponse>.Ok(limite));
    }

    /// <summary>
    /// Obtener límite de una categoría en un período específico
    /// </summary>
    [HttpGet("categoria/{categoriaId:int}/{anio:int}/{mes:int}")]
    [ProducesResponseType(typeof(ApiResponse<LimiteCategoriaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<LimiteCategoriaResponse>>> GetByCategoriaYPeriodo(
        int categoriaId, int anio, int mes)
    {
        if (mes < 1 || mes > 12)
        {
            return BadRequest(ApiResponse.Fail("El mes debe estar entre 1 y 12"));
        }

        var limite = await _limitesService.GetByCategoriaYPeriodoAsync(categoriaId, anio, mes);

        if (limite == null)
        {
            return NotFound(ApiResponse.Fail("No existe límite para esta categoría en el período especificado"));
        }

        return Ok(ApiResponse<LimiteCategoriaResponse>.Ok(limite));
    }

    /// <summary>
    /// Obtener histórico de límites de una categoría
    /// </summary>
    [HttpGet("historico/{categoriaId:int}")]
    [ProducesResponseType(typeof(ApiResponse<HistoricoLimiteCategoriaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<HistoricoLimiteCategoriaResponse>>> GetHistoricoByCategoria(int categoriaId)
    {
        try
        {
            var historico = await _limitesService.GetHistoricoByCategoriaAsync(categoriaId);
            return Ok(ApiResponse<HistoricoLimiteCategoriaResponse>.Ok(historico));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Crear o actualizar un límite
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<LimiteCategoriaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<LimiteCategoriaResponse>>> CreateOrUpdate(
        [FromBody] CreateLimiteCategoriaRequest request)
    {
        try
        {
            var limite = await _limitesService.CreateOrUpdateAsync(request);
            return Ok(ApiResponse<LimiteCategoriaResponse>.Ok(limite, "Límite guardado exitosamente"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Crear múltiples límites en lote para un período
    /// </summary>
    [HttpPost("lote")]
    [ProducesResponseType(typeof(ApiResponse<ResumenLimitesPeriodoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ResumenLimitesPeriodoResponse>>> CreateLote(
        [FromBody] CreateLimitesLoteRequest request)
    {
        try
        {
            var resumen = await _limitesService.CreateLoteAsync(request);
            return Ok(ApiResponse<ResumenLimitesPeriodoResponse>.Ok(resumen, "Límites guardados exitosamente"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Actualizar el monto de un límite existente
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<LimiteCategoriaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<LimiteCategoriaResponse>>> Update(
        int id, [FromBody] UpdateLimiteCategoriaRequest request)
    {
        var limite = await _limitesService.UpdateAsync(id, request);

        if (limite == null)
        {
            return NotFound(ApiResponse.Fail("Límite no encontrado"));
        }

        return Ok(ApiResponse<LimiteCategoriaResponse>.Ok(limite, "Límite actualizado exitosamente"));
    }

    /// <summary>
    /// Eliminar un límite
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> Delete(int id)
    {
        var result = await _limitesService.DeleteAsync(id);

        if (!result)
        {
            return NotFound(ApiResponse.Fail("Límite no encontrado"));
        }

        return Ok(ApiResponse.Ok("Límite eliminado exitosamente"));
    }

    /// <summary>
    /// Copiar límites de un período a otro
    /// </summary>
    [HttpPost("copiar/{anioOrigen:int}/{mesOrigen:int}/a/{anioDestino:int}/{mesDestino:int}")]
    [ProducesResponseType(typeof(ApiResponse<ResumenLimitesPeriodoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ResumenLimitesPeriodoResponse>>> CopiarPeriodo(
        int anioOrigen, int mesOrigen, int anioDestino, int mesDestino)
    {
        if (mesOrigen < 1 || mesOrigen > 12 || mesDestino < 1 || mesDestino > 12)
        {
            return BadRequest(ApiResponse.Fail("Los meses deben estar entre 1 y 12"));
        }

        try
        {
            var resumen = await _limitesService.CopiarPeriodoAsync(anioOrigen, mesOrigen, anioDestino, mesDestino);
            return Ok(ApiResponse<ResumenLimitesPeriodoResponse>.Ok(resumen, "Límites copiados exitosamente"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }
}
