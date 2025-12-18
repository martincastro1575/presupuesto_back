using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlanificadorGastos.API.Models.Common;
using PlanificadorGastos.API.Models.DTOs.Gastos;
using PlanificadorGastos.API.Services.Interfaces;

namespace PlanificadorGastos.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GastosController : ControllerBase
{
    private readonly IGastosService _gastosService;

    public GastosController(IGastosService gastosService)
    {
        _gastosService = gastosService;
    }

    /// <summary>
    /// Obtener gastos con paginación y filtros
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<GastoResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaginatedResult<GastoResponse>>>> GetAll([FromQuery] GastosFilterParams filterParams)
    {
        var gastos = await _gastosService.GetAllAsync(filterParams);
        return Ok(ApiResponse<PaginatedResult<GastoResponse>>.Ok(gastos));
    }

    /// <summary>
    /// Obtener un gasto por ID
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<GastoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<GastoResponse>>> GetById(int id)
    {
        var gasto = await _gastosService.GetByIdAsync(id);
        
        if (gasto == null)
        {
            return NotFound(ApiResponse.Fail("Gasto no encontrado"));
        }

        return Ok(ApiResponse<GastoResponse>.Ok(gasto));
    }

    /// <summary>
    /// Crear un nuevo gasto
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<GastoResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<GastoResponse>>> Create([FromBody] CreateGastoRequest request)
    {
        var gasto = await _gastosService.CreateAsync(request);
        return CreatedAtAction(
            nameof(GetById), 
            new { id = gasto.Id }, 
            ApiResponse<GastoResponse>.Ok(gasto, "Gasto registrado exitosamente"));
    }

    /// <summary>
    /// Actualizar un gasto existente
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<GastoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<GastoResponse>>> Update(int id, [FromBody] UpdateGastoRequest request)
    {
        var gasto = await _gastosService.UpdateAsync(id, request);
        return Ok(ApiResponse<GastoResponse>.Ok(gasto, "Gasto actualizado exitosamente"));
    }

    /// <summary>
    /// Eliminar un gasto
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> Delete(int id)
    {
        var result = await _gastosService.DeleteAsync(id);
        
        if (!result)
        {
            return NotFound(ApiResponse.Fail("Gasto no encontrado"));
        }

        return Ok(ApiResponse.Ok("Gasto eliminado exitosamente"));
    }
}
