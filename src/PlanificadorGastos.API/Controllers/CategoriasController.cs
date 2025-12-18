using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlanificadorGastos.API.Models.Common;
using PlanificadorGastos.API.Models.DTOs.Categorias;
using PlanificadorGastos.API.Services.Interfaces;

namespace PlanificadorGastos.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriasController : ControllerBase
{
    private readonly ICategoriasService _categoriasService;

    public CategoriasController(ICategoriasService categoriasService)
    {
        _categoriasService = categoriasService;
    }

    /// <summary>
    /// Obtener todas las categorías (predefinidas + del usuario)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CategoriaResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<CategoriaResponse>>>> GetAll()
    {
        var categorias = await _categoriasService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<CategoriaResponse>>.Ok(categorias));
    }

    /// <summary>
    /// Obtener una categoría por ID
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CategoriaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CategoriaResponse>>> GetById(int id)
    {
        var categoria = await _categoriasService.GetByIdAsync(id);
        
        if (categoria == null)
        {
            return NotFound(ApiResponse.Fail("Categoría no encontrada"));
        }

        return Ok(ApiResponse<CategoriaResponse>.Ok(categoria));
    }

    /// <summary>
    /// Crear una nueva categoría
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CategoriaResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<CategoriaResponse>>> Create([FromBody] CreateCategoriaRequest request)
    {
        var categoria = await _categoriasService.CreateAsync(request);
        return CreatedAtAction(
            nameof(GetById), 
            new { id = categoria.Id }, 
            ApiResponse<CategoriaResponse>.Ok(categoria, "Categoría creada exitosamente"));
    }

    /// <summary>
    /// Actualizar una categoría existente
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CategoriaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<CategoriaResponse>>> Update(int id, [FromBody] UpdateCategoriaRequest request)
    {
        var categoria = await _categoriasService.UpdateAsync(id, request);
        return Ok(ApiResponse<CategoriaResponse>.Ok(categoria, "Categoría actualizada exitosamente"));
    }

    /// <summary>
    /// Eliminar una categoría
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> Delete(int id)
    {
        var result = await _categoriasService.DeleteAsync(id);
        
        if (!result)
        {
            return NotFound(ApiResponse.Fail("Categoría no encontrada o no se puede eliminar"));
        }

        return Ok(ApiResponse.Ok("Categoría eliminada exitosamente"));
    }
}
