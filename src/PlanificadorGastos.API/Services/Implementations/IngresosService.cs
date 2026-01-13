using Microsoft.EntityFrameworkCore;
using PlanificadorGastos.API.Data;
using PlanificadorGastos.API.Models.DTOs.Ingresos;
using PlanificadorGastos.API.Models.Entities;
using PlanificadorGastos.API.Services.Interfaces;

namespace PlanificadorGastos.API.Services.Implementations;

public class IngresosService : IIngresosService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public IngresosService(ApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    private int UserId => _currentUserService.UserId
        ?? throw new UnauthorizedAccessException("Usuario no autenticado");

    public async Task<IEnumerable<IngresoResponse>> GetAllAsync()
    {
        var ingresos = await _context.Ingresos
            .Where(i => i.UserId == UserId)
            .Include(i => i.Categoria)
            .OrderByDescending(i => i.Fecha)
            .Select(i => new IngresoResponse
            {
                Id = i.Id,
                CategoriaId = i.CategoriaId,
                CategoriaNombre = i.Categoria.Nombre,
                CategoriaIcono = i.Categoria.Icono,
                CategoriaColor = i.Categoria.Color,
                Monto = i.Monto,
                Concepto = i.Concepto,
                Fecha = i.Fecha,
                Descripcion = i.Descripcion
            })
            .ToListAsync();

        return ingresos;
    }

    public async Task<IEnumerable<IngresoResponse>> GetByPeriodoAsync(int anio, int mes)
    {
        var ingresos = await _context.Ingresos
            .Where(i => i.UserId == UserId && i.Fecha.Year == anio && i.Fecha.Month == mes)
            .Include(i => i.Categoria)
            .OrderByDescending(i => i.Fecha)
            .Select(i => new IngresoResponse
            {
                Id = i.Id,
                CategoriaId = i.CategoriaId,
                CategoriaNombre = i.Categoria.Nombre,
                CategoriaIcono = i.Categoria.Icono,
                CategoriaColor = i.Categoria.Color,
                Monto = i.Monto,
                Concepto = i.Concepto,
                Fecha = i.Fecha,
                Descripcion = i.Descripcion
            })
            .ToListAsync();

        return ingresos;
    }

    public async Task<IngresoResponse?> GetByIdAsync(int id)
    {
        var ingreso = await _context.Ingresos
            .Where(i => i.Id == id && i.UserId == UserId)
            .Include(i => i.Categoria)
            .Select(i => new IngresoResponse
            {
                Id = i.Id,
                CategoriaId = i.CategoriaId,
                CategoriaNombre = i.Categoria.Nombre,
                CategoriaIcono = i.Categoria.Icono,
                CategoriaColor = i.Categoria.Color,
                Monto = i.Monto,
                Concepto = i.Concepto,
                Fecha = i.Fecha,
                Descripcion = i.Descripcion
            })
            .FirstOrDefaultAsync();

        return ingreso;
    }

    public async Task<IngresoResponse> CreateAsync(CreateIngresoRequest request)
    {
        // Validar que la categoría exista, sea de tipo Ingreso o Ambos, y sea accesible
        var categoriaValida = await _context.Categorias
            .AnyAsync(c => c.Id == request.CategoriaId &&
                          (c.EsPredefinida || c.UserId == UserId) &&
                          (c.Tipo == TipoCategoria.Ingreso || c.Tipo == TipoCategoria.Ambos) &&
                          c.Activa);

        if (!categoriaValida)
        {
            throw new ArgumentException("La categoría seleccionada no es válida para ingresos");
        }

        var ingreso = new Ingreso
        {
            UserId = UserId,
            CategoriaId = request.CategoriaId,
            Monto = request.Monto,
            Concepto = request.Concepto,
            Fecha = request.Fecha,
            Descripcion = request.Descripcion,
            CreatedAt = DateTime.UtcNow
        };

        _context.Ingresos.Add(ingreso);
        await _context.SaveChangesAsync();

        // Cargar la categoría para la respuesta
        await _context.Entry(ingreso).Reference(i => i.Categoria).LoadAsync();

        return new IngresoResponse
        {
            Id = ingreso.Id,
            CategoriaId = ingreso.CategoriaId,
            CategoriaNombre = ingreso.Categoria.Nombre,
            CategoriaIcono = ingreso.Categoria.Icono,
            CategoriaColor = ingreso.Categoria.Color,
            Monto = ingreso.Monto,
            Concepto = ingreso.Concepto,
            Fecha = ingreso.Fecha,
            Descripcion = ingreso.Descripcion
        };
    }

    public async Task<IngresoResponse?> UpdateAsync(int id, UpdateIngresoRequest request)
    {
        var ingreso = await _context.Ingresos
            .FirstOrDefaultAsync(i => i.Id == id && i.UserId == UserId);

        if (ingreso == null)
        {
            return null;
        }

        // Validar categoría
        var categoriaValida = await _context.Categorias
            .AnyAsync(c => c.Id == request.CategoriaId &&
                          (c.EsPredefinida || c.UserId == UserId) &&
                          (c.Tipo == TipoCategoria.Ingreso || c.Tipo == TipoCategoria.Ambos) &&
                          c.Activa);

        if (!categoriaValida)
        {
            throw new ArgumentException("La categoría seleccionada no es válida para ingresos");
        }

        ingreso.CategoriaId = request.CategoriaId;
        ingreso.Monto = request.Monto;
        ingreso.Concepto = request.Concepto;
        ingreso.Fecha = request.Fecha;
        ingreso.Descripcion = request.Descripcion;

        await _context.SaveChangesAsync();

        // Cargar la categoría para la respuesta
        await _context.Entry(ingreso).Reference(i => i.Categoria).LoadAsync();

        return new IngresoResponse
        {
            Id = ingreso.Id,
            CategoriaId = ingreso.CategoriaId,
            CategoriaNombre = ingreso.Categoria.Nombre,
            CategoriaIcono = ingreso.Categoria.Icono,
            CategoriaColor = ingreso.Categoria.Color,
            Monto = ingreso.Monto,
            Concepto = ingreso.Concepto,
            Fecha = ingreso.Fecha,
            Descripcion = ingreso.Descripcion
        };
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var ingreso = await _context.Ingresos
            .FirstOrDefaultAsync(i => i.Id == id && i.UserId == UserId);

        if (ingreso == null)
        {
            return false;
        }

        _context.Ingresos.Remove(ingreso);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<decimal> GetTotalByPeriodoAsync(int anio, int mes)
    {
        var total = await _context.Ingresos
            .Where(i => i.UserId == UserId && i.Fecha.Year == anio && i.Fecha.Month == mes)
            .SumAsync(i => i.Monto);

        return total;
    }
}
