using Microsoft.EntityFrameworkCore;
using PlanificadorGastos.API.Data;
using PlanificadorGastos.API.Models.DTOs.Presupuestos;
using PlanificadorGastos.API.Models.Entities;
using PlanificadorGastos.API.Services.Interfaces;

namespace PlanificadorGastos.API.Services.Implementations;

public class PresupuestosService : IPresupuestosService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public PresupuestosService(ApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    private int UserId => _currentUserService.UserId 
        ?? throw new UnauthorizedAccessException("Usuario no autenticado");

    public async Task<IEnumerable<PresupuestoResponse>> GetAllAsync()
    {
        var presupuestos = await GetPresupuestosConGastosQuery()
            .OrderByDescending(p => p.Anio)
            .ThenByDescending(p => p.Mes)
            .ToListAsync();

        return presupuestos;
    }

    public async Task<IEnumerable<PresupuestoResponse>> GetByPeriodoAsync(int anio, int mes)
    {
        var presupuestos = await GetPresupuestosConGastosQuery()
            .Where(p => p.Anio == anio && p.Mes == mes)
            .ToListAsync();

        return presupuestos;
    }

    public async Task<PresupuestoResponse?> GetByIdAsync(int id)
    {
        var presupuesto = await GetPresupuestosConGastosQuery()
            .FirstOrDefaultAsync(p => p.Id == id);

        return presupuesto;
    }

    public async Task<PresupuestoResponse> CreateOrUpdateAsync(CreatePresupuestoRequest request)
    {
        Presupuesto? existente = null;

        // Si viene Id, buscar por Id para actualizar todos los campos
        if (request.Id.HasValue)
        {
            existente = await _context.Presupuestos
                .FirstOrDefaultAsync(p => p.Id == request.Id.Value && p.UserId == UserId);

            if (existente == null)
            {
                throw new ArgumentException("El presupuesto no existe o no tienes permisos para modificarlo");
            }

            // Validar categoría si se especificó
            if (request.CategoriaId.HasValue)
            {
                var categoriaValida = await _context.Categorias
                    .AnyAsync(c => c.Id == request.CategoriaId.Value &&
                                  (c.EsPredefinida || c.UserId == UserId));

                if (!categoriaValida)
                {
                    throw new ArgumentException("La categoría seleccionada no es válida");
                }
            }

            // Actualizar todos los campos
            existente.CategoriaId = request.CategoriaId;
            existente.MontoLimite = request.MontoLimite;
            existente.Anio = request.Anio;
            existente.Mes = request.Mes;
            await _context.SaveChangesAsync();
        }
        else
        {
            // Verificar si ya existe un presupuesto para este período/categoría
            existente = await _context.Presupuestos
                .FirstOrDefaultAsync(p => p.UserId == UserId &&
                                          p.CategoriaId == request.CategoriaId &&
                                          p.Anio == request.Anio &&
                                          p.Mes == request.Mes);

            if (existente != null)
            {
                // Actualizar existente (solo monto, ya que coincide categoría/período)
                existente.MontoLimite = request.MontoLimite;
                await _context.SaveChangesAsync();
            }
            else
            {
                // Validar categoría si se especificó
                if (request.CategoriaId.HasValue)
                {
                    var categoriaValida = await _context.Categorias
                        .AnyAsync(c => c.Id == request.CategoriaId.Value &&
                                      (c.EsPredefinida || c.UserId == UserId));

                    if (!categoriaValida)
                    {
                        throw new ArgumentException("La categoría seleccionada no es válida");
                    }
                }

                existente = new Presupuesto
                {
                    UserId = UserId,
                    CategoriaId = request.CategoriaId,
                    MontoLimite = request.MontoLimite,
                    Anio = request.Anio,
                    Mes = request.Mes,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Presupuestos.Add(existente);
                await _context.SaveChangesAsync();
            }
        }

        // Retornar con el gasto calculado
        var resultado = await GetPresupuestosConGastosQuery()
            .FirstAsync(p => p.Id == existente.Id);

        return resultado;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var presupuesto = await _context.Presupuestos
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == UserId);

        if (presupuesto == null)
        {
            return false;
        }

        _context.Presupuestos.Remove(presupuesto);
        await _context.SaveChangesAsync();

        return true;
    }

    private IQueryable<PresupuestoResponse> GetPresupuestosConGastosQuery()
    {
        return from p in _context.Presupuestos.Where(p => p.UserId == UserId)
               join c in _context.Categorias on p.CategoriaId equals c.Id into categorias
               from c in categorias.DefaultIfEmpty()
               let montoGastado = _context.Gastos
                   .Where(g => g.UserId == UserId &&
                               g.Fecha.Year == p.Anio &&
                               g.Fecha.Month == p.Mes &&
                               (p.CategoriaId == null || g.CategoriaId == p.CategoriaId))
                   .Sum(g => (decimal?)g.Monto) ?? 0
               select new PresupuestoResponse
               {
                   Id = p.Id,
                   CategoriaId = p.CategoriaId,
                   CategoriaNombre = c != null ? c.Nombre : "General",
                   CategoriaColor = c != null ? c.Color : "#6366f1",
                   MontoLimite = p.MontoLimite,
                   Anio = p.Anio,
                   Mes = p.Mes,
                   MontoGastado = montoGastado
               };
    }
}
