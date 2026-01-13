using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlanificadorGastos.API.Models.Common;
using PlanificadorGastos.API.Models.DTOs.Ingresos;
using PlanificadorGastos.API.Services.Interfaces;

namespace PlanificadorGastos.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class IngresosController : ControllerBase
{
    private readonly IIngresosService _ingresosService;

    public IngresosController(IIngresosService ingresosService)
    {
        _ingresosService = ingresosService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<IngresoResponse>>>> GetAll()
    {
        var ingresos = await _ingresosService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<IngresoResponse>>.Ok(ingresos));
    }

    [HttpGet("periodo/{anio}/{mes}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<IngresoResponse>>>> GetByPeriodo(int anio, int mes)
    {
        if (mes < 1 || mes > 12)
        {
            return BadRequest(ApiResponse<IEnumerable<IngresoResponse>>.Fail("El mes debe estar entre 1 y 12"));
        }

        var ingresos = await _ingresosService.GetByPeriodoAsync(anio, mes);
        return Ok(ApiResponse<IEnumerable<IngresoResponse>>.Ok(ingresos));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<IngresoResponse>>> GetById(int id)
    {
        var ingreso = await _ingresosService.GetByIdAsync(id);

        if (ingreso == null)
        {
            return NotFound(ApiResponse<IngresoResponse>.Fail("Ingreso no encontrado"));
        }

        return Ok(ApiResponse<IngresoResponse>.Ok(ingreso));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<IngresoResponse>>> Create([FromBody] CreateIngresoRequest request)
    {
        var ingreso = await _ingresosService.CreateAsync(request);
        return CreatedAtAction(
            nameof(GetById),
            new { id = ingreso.Id },
            ApiResponse<IngresoResponse>.Ok(ingreso, "Ingreso creado correctamente")
        );
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<IngresoResponse>>> Update(int id, [FromBody] UpdateIngresoRequest request)
    {
        var ingreso = await _ingresosService.UpdateAsync(id, request);

        if (ingreso == null)
        {
            return NotFound(ApiResponse<IngresoResponse>.Fail("Ingreso no encontrado"));
        }

        return Ok(ApiResponse<IngresoResponse>.Ok(ingreso, "Ingreso actualizado correctamente"));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        var result = await _ingresosService.DeleteAsync(id);

        if (!result)
        {
            return NotFound(ApiResponse<object>.Fail("Ingreso no encontrado"));
        }

        return Ok(ApiResponse<object>.Ok(null!, "Ingreso eliminado correctamente"));
    }

    [HttpGet("total/{anio}/{mes}")]
    public async Task<ActionResult<ApiResponse<decimal>>> GetTotalByPeriodo(int anio, int mes)
    {
        if (mes < 1 || mes > 12)
        {
            return BadRequest(ApiResponse<decimal>.Fail("El mes debe estar entre 1 y 12"));
        }

        var total = await _ingresosService.GetTotalByPeriodoAsync(anio, mes);
        return Ok(ApiResponse<decimal>.Ok(total));
    }
}
