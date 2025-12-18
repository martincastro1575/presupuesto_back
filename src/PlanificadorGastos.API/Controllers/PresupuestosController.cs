using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlanificadorGastos.API.Models.Common;
using PlanificadorGastos.API.Models.DTOs.Presupuestos;
using PlanificadorGastos.API.Services.Interfaces;

namespace PlanificadorGastos.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PresupuestosController : ControllerBase
{
    private readonly IPresupuestosService _presupuestosService;

    public PresupuestosController(IPresupuestosService presupuestosService)
    {
        _presupuestosService = presupuestosService;
    }

    /// <summary>
    /// Obtener todos los presupuestos del usuario
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PresupuestoResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PresupuestoResponse>>>> GetAll()
    {
        var presupuestos = await _presupuestosService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<PresupuestoResponse>>.Ok(presupuestos));
    }

    /// <summary>
    /// Obtener presupuestos de un período específico
    /// </summary>
    [HttpGet("{anio:int}/{mes:int}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PresupuestoResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PresupuestoResponse>>>> GetByPeriodo(int anio, int mes)
    {
        if (mes < 1 || mes > 12)
        {
            return BadRequest(ApiResponse.Fail("El mes debe estar entre 1 y 12"));
        }

        var presupuestos = await _presupuestosService.GetByPeriodoAsync(anio, mes);
        return Ok(ApiResponse<IEnumerable<PresupuestoResponse>>.Ok(presupuestos));
    }

    /// <summary>
    /// Crear o actualizar un presupuesto
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<PresupuestoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<PresupuestoResponse>>> CreateOrUpdate([FromBody] CreatePresupuestoRequest request)
    {
        var presupuesto = await _presupuestosService.CreateOrUpdateAsync(request);
        return Ok(ApiResponse<PresupuestoResponse>.Ok(presupuesto, "Presupuesto guardado exitosamente"));
    }

    /// <summary>
    /// Eliminar un presupuesto
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> Delete(int id)
    {
        var result = await _presupuestosService.DeleteAsync(id);
        
        if (!result)
        {
            return NotFound(ApiResponse.Fail("Presupuesto no encontrado"));
        }

        return Ok(ApiResponse.Ok("Presupuesto eliminado exitosamente"));
    }
}
