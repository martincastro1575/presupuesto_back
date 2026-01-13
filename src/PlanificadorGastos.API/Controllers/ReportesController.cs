using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlanificadorGastos.API.Models.Common;
using PlanificadorGastos.API.Models.DTOs.Reportes;
using PlanificadorGastos.API.Services.Interfaces;

namespace PlanificadorGastos.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportesController : ControllerBase
{
    private readonly IReportesService _reportesService;

    public ReportesController(IReportesService reportesService)
    {
        _reportesService = reportesService;
    }

    /// <summary>
    /// Obtener resumen del mes actual o especificado
    /// </summary>
    [HttpGet("resumen-mensual")]
    [ProducesResponseType(typeof(ApiResponse<ResumenMensualResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ResumenMensualResponse>>> GetResumenMensual(
        [FromQuery] int? anio = null, 
        [FromQuery] int? mes = null)
    {
        var resumen = await _reportesService.GetResumenMensualAsync(anio, mes);
        return Ok(ApiResponse<ResumenMensualResponse>.Ok(resumen));
    }

    /// <summary>
    /// Obtener gastos agrupados por categoría
    /// </summary>
    [HttpGet("por-categoria")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<GastoPorCategoriaResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<GastoPorCategoriaResponse>>>> GetPorCategoria(
        [FromQuery] int? anio = null,
        [FromQuery] int? mes = null)
    {
        var gastos = await _reportesService.GetGastosPorCategoriaAsync(anio, mes);
        return Ok(ApiResponse<IEnumerable<GastoPorCategoriaResponse>>.Ok(gastos));
    }

    /// <summary>
    /// Obtener ingresos agrupados por categoría
    /// </summary>
    [HttpGet("ingresos-por-categoria")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<IngresoPorCategoriaResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<IngresoPorCategoriaResponse>>>> GetIngresosPorCategoria(
        [FromQuery] int? anio = null,
        [FromQuery] int? mes = null)
    {
        var ingresos = await _reportesService.GetIngresosPorCategoriaAsync(anio, mes);
        return Ok(ApiResponse<IEnumerable<IngresoPorCategoriaResponse>>.Ok(ingresos));
    }

    /// <summary>
    /// Obtener evolución de gastos de los últimos N meses
    /// </summary>
    [HttpGet("evolucion")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<EvolucionMensualResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<EvolucionMensualResponse>>>> GetEvolucion(
        [FromQuery] int meses = 6)
    {
        if (meses < 1 || meses > 24)
        {
            return BadRequest(ApiResponse.Fail("La cantidad de meses debe estar entre 1 y 24"));
        }

        var evolucion = await _reportesService.GetEvolucionMensualAsync(meses);
        return Ok(ApiResponse<IEnumerable<EvolucionMensualResponse>>.Ok(evolucion));
    }

    /// <summary>
    /// Obtener comparativo entre mes actual y anterior
    /// </summary>
    [HttpGet("comparativo")]
    [ProducesResponseType(typeof(ApiResponse<ComparativoMensualResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ComparativoMensualResponse>>> GetComparativo()
    {
        var comparativo = await _reportesService.GetComparativoMensualAsync();
        return Ok(ApiResponse<ComparativoMensualResponse>.Ok(comparativo));
    }
}
