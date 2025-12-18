using Microsoft.EntityFrameworkCore;
using PlanificadorGastos.API.Data;
using PlanificadorGastos.API.Models.Common;
using PlanificadorGastos.API.Models.DTOs.Gastos;
using PlanificadorGastos.API.Models.Entities;
using PlanificadorGastos.API.Services.Interfaces;

namespace PlanificadorGastos.API.Services.Implementations;

public class GastosService : IGastosService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GastosService(ApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    private int UserId => _currentUserService.UserId 
        ?? throw new UnauthorizedAccessException("Usuario no autenticado");

    public async Task<PaginatedResult<GastoResponse>> GetAllAsync(GastosFilterParams filterParams)
    {
        var query = _context.Gastos
            .Include(g => g.Categoria)
            .Where(g => g.UserId == UserId)
            .AsQueryable();

        // Aplicar filtros
        if (filterParams.FechaDesde.HasValue)
        {
            query = query.Where(g => g.Fecha >= filterParams.FechaDesde.Value);
        }

        if (filterParams.FechaHasta.HasValue)
        {
            query = query.Where(g => g.Fecha <= filterParams.FechaHasta.Value);
        }

        if (filterParams.CategoriaId.HasValue)
        {
            query = query.Where(g => g.CategoriaId == filterParams.CategoriaId.Value);
        }

        // Obtener total
        var totalCount = await query.CountAsync();

        // Aplicar ordenamiento
        query = filterParams.OrdenarPor?.ToLower() switch
        {
            "monto" => filterParams.OrdenDesc 
                ? query.OrderByDescending(g => g.Monto) 
                : query.OrderBy(g => g.Monto),
            "categoria" => filterParams.OrdenDesc 
                ? query.OrderByDescending(g => g.Categoria.Nombre) 
                : query.OrderBy(g => g.Categoria.Nombre),
            _ => filterParams.OrdenDesc 
                ? query.OrderByDescending(g => g.Fecha).ThenByDescending(g => g.CreatedAt) 
                : query.OrderBy(g => g.Fecha).ThenBy(g => g.CreatedAt)
        };

        // Aplicar paginación
        var items = await query
            .Skip((filterParams.Page - 1) * filterParams.PageSize)
            .Take(filterParams.PageSize)
            .Select(g => new GastoResponse
            {
                Id = g.Id,
                CategoriaId = g.CategoriaId,
                CategoriaNombre = g.Categoria.Nombre,
                CategoriaIcono = g.Categoria.Icono,
                CategoriaColor = g.Categoria.Color,
                Monto = g.Monto,
                Fecha = g.Fecha,
                Descripcion = g.Descripcion,
                CreatedAt = g.CreatedAt
            })
            .ToListAsync();

        return PaginatedResult<GastoResponse>.Create(items, filterParams.Page, filterParams.PageSize, totalCount);
    }

    public async Task<GastoResponse?> GetByIdAsync(int id)
    {
        var gasto = await _context.Gastos
            .Include(g => g.Categoria)
            .Where(g => g.Id == id && g.UserId == UserId)
            .Select(g => new GastoResponse
            {
                Id = g.Id,
                CategoriaId = g.CategoriaId,
                CategoriaNombre = g.Categoria.Nombre,
                CategoriaIcono = g.Categoria.Icono,
                CategoriaColor = g.Categoria.Color,
                Monto = g.Monto,
                Fecha = g.Fecha,
                Descripcion = g.Descripcion,
                CreatedAt = g.CreatedAt
            })
            .FirstOrDefaultAsync();

        return gasto;
    }

    public async Task<GastoResponse> CreateAsync(CreateGastoRequest request)
    {
        // Verificar que la categoría existe y es accesible
        var categoriaValida = await _context.Categorias
            .AnyAsync(c => c.Id == request.CategoriaId && 
                          (c.EsPredefinida || c.UserId == UserId) && 
                          c.Activa);

        if (!categoriaValida)
        {
            throw new ArgumentException("La categoría seleccionada no es válida");
        }

        var gasto = new Gasto
        {
            UserId = UserId,
            CategoriaId = request.CategoriaId,
            Monto = request.Monto,
            Fecha = request.Fecha,
            Descripcion = request.Descripcion,
            CreatedAt = DateTime.UtcNow
        };

        _context.Gastos.Add(gasto);
        await _context.SaveChangesAsync();

        // Cargar la categoría para la respuesta
        await _context.Entry(gasto).Reference(g => g.Categoria).LoadAsync();

        return new GastoResponse
        {
            Id = gasto.Id,
            CategoriaId = gasto.CategoriaId,
            CategoriaNombre = gasto.Categoria.Nombre,
            CategoriaIcono = gasto.Categoria.Icono,
            CategoriaColor = gasto.Categoria.Color,
            Monto = gasto.Monto,
            Fecha = gasto.Fecha,
            Descripcion = gasto.Descripcion,
            CreatedAt = gasto.CreatedAt
        };
    }

    public async Task<GastoResponse> UpdateAsync(int id, UpdateGastoRequest request)
    {
        var gasto = await _context.Gastos
            .Include(g => g.Categoria)
            .FirstOrDefaultAsync(g => g.Id == id && g.UserId == UserId);

        if (gasto == null)
        {
            throw new KeyNotFoundException("Gasto no encontrado");
        }

        // Verificar categoría si cambió
        if (gasto.CategoriaId != request.CategoriaId)
        {
            var categoriaValida = await _context.Categorias
                .AnyAsync(c => c.Id == request.CategoriaId && 
                              (c.EsPredefinida || c.UserId == UserId) && 
                              c.Activa);

            if (!categoriaValida)
            {
                throw new ArgumentException("La categoría seleccionada no es válida");
            }
        }

        gasto.CategoriaId = request.CategoriaId;
        gasto.Monto = request.Monto;
        gasto.Fecha = request.Fecha;
        gasto.Descripcion = request.Descripcion;
        gasto.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Recargar categoría si cambió
        await _context.Entry(gasto).Reference(g => g.Categoria).LoadAsync();

        return new GastoResponse
        {
            Id = gasto.Id,
            CategoriaId = gasto.CategoriaId,
            CategoriaNombre = gasto.Categoria.Nombre,
            CategoriaIcono = gasto.Categoria.Icono,
            CategoriaColor = gasto.Categoria.Color,
            Monto = gasto.Monto,
            Fecha = gasto.Fecha,
            Descripcion = gasto.Descripcion,
            CreatedAt = gasto.CreatedAt
        };
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var gasto = await _context.Gastos
            .FirstOrDefaultAsync(g => g.Id == id && g.UserId == UserId);

        if (gasto == null)
        {
            return false;
        }

        _context.Gastos.Remove(gasto);
        await _context.SaveChangesAsync();

        return true;
    }
}
