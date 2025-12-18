using Microsoft.EntityFrameworkCore;
using PlanificadorGastos.API.Data;
using PlanificadorGastos.API.Models.DTOs.Categorias;
using PlanificadorGastos.API.Models.Entities;
using PlanificadorGastos.API.Services.Interfaces;

namespace PlanificadorGastos.API.Services.Implementations;

public class CategoriasService : ICategoriasService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CategoriasService(ApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    private int UserId => _currentUserService.UserId 
        ?? throw new UnauthorizedAccessException("Usuario no autenticado");

    public async Task<IEnumerable<CategoriaResponse>> GetAllAsync()
    {
        // Obtener categorías predefinidas + categorías del usuario
        var categorias = await _context.Categorias
            .Where(c => c.EsPredefinida || c.UserId == UserId)
            .Where(c => c.Activa)
            .OrderBy(c => c.EsPredefinida ? 0 : 1)
            .ThenBy(c => c.Nombre)
            .Select(c => new CategoriaResponse
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Icono = c.Icono,
                Color = c.Color,
                EsPredefinida = c.EsPredefinida,
                Activa = c.Activa
            })
            .ToListAsync();

        return categorias;
    }

    public async Task<CategoriaResponse?> GetByIdAsync(int id)
    {
        var categoria = await _context.Categorias
            .Where(c => c.Id == id && (c.EsPredefinida || c.UserId == UserId))
            .Select(c => new CategoriaResponse
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Icono = c.Icono,
                Color = c.Color,
                EsPredefinida = c.EsPredefinida,
                Activa = c.Activa
            })
            .FirstOrDefaultAsync();

        return categoria;
    }

    public async Task<CategoriaResponse> CreateAsync(CreateCategoriaRequest request)
    {
        // Verificar si ya existe una categoría con el mismo nombre para el usuario
        var existe = await _context.Categorias
            .AnyAsync(c => c.UserId == UserId && c.Nombre.ToLower() == request.Nombre.ToLower());

        if (existe)
        {
            throw new InvalidOperationException($"Ya existe una categoría con el nombre '{request.Nombre}'");
        }

        var categoria = new Categoria
        {
            UserId = UserId,
            Nombre = request.Nombre,
            Icono = request.Icono,
            Color = request.Color,
            EsPredefinida = false,
            Activa = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Categorias.Add(categoria);
        await _context.SaveChangesAsync();

        return new CategoriaResponse
        {
            Id = categoria.Id,
            Nombre = categoria.Nombre,
            Icono = categoria.Icono,
            Color = categoria.Color,
            EsPredefinida = categoria.EsPredefinida,
            Activa = categoria.Activa
        };
    }

    public async Task<CategoriaResponse> UpdateAsync(int id, UpdateCategoriaRequest request)
    {
        var categoria = await _context.Categorias
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == UserId && !c.EsPredefinida);

        if (categoria == null)
        {
            throw new KeyNotFoundException("Categoría no encontrada o no se puede modificar");
        }

        // Verificar nombre duplicado
        var existe = await _context.Categorias
            .AnyAsync(c => c.Id != id && c.UserId == UserId && c.Nombre.ToLower() == request.Nombre.ToLower());

        if (existe)
        {
            throw new InvalidOperationException($"Ya existe una categoría con el nombre '{request.Nombre}'");
        }

        categoria.Nombre = request.Nombre;
        categoria.Icono = request.Icono;
        categoria.Color = request.Color;
        categoria.Activa = request.Activa;

        await _context.SaveChangesAsync();

        return new CategoriaResponse
        {
            Id = categoria.Id,
            Nombre = categoria.Nombre,
            Icono = categoria.Icono,
            Color = categoria.Color,
            EsPredefinida = categoria.EsPredefinida,
            Activa = categoria.Activa
        };
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var categoria = await _context.Categorias
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == UserId && !c.EsPredefinida);

        if (categoria == null)
        {
            return false;
        }

        // Verificar si tiene gastos asociados
        var tieneGastos = await _context.Gastos.AnyAsync(g => g.CategoriaId == id);

        if (tieneGastos)
        {
            // En lugar de eliminar, desactivar
            categoria.Activa = false;
            await _context.SaveChangesAsync();
        }
        else
        {
            _context.Categorias.Remove(categoria);
            await _context.SaveChangesAsync();
        }

        return true;
    }
}
