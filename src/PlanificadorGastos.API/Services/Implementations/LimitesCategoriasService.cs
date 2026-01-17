using Microsoft.EntityFrameworkCore;
using PlanificadorGastos.API.Data;
using PlanificadorGastos.API.Models.DTOs.LimitesCategorias;
using PlanificadorGastos.API.Models.Entities;
using PlanificadorGastos.API.Services.Interfaces;

namespace PlanificadorGastos.API.Services.Implementations;

public class LimitesCategoriasService : ILimitesCategoriasService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public LimitesCategoriasService(
        ApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    private int UserId => _currentUserService.UserId
        ?? throw new UnauthorizedAccessException("Usuario no autenticado");

    public async Task<IEnumerable<LimiteCategoriaResponse>> GetAllAsync()
    {
        return await GetLimitesConGastosQuery()
            .OrderByDescending(l => l.Anio)
            .ThenByDescending(l => l.Mes)
            .ThenBy(l => l.CategoriaNombre)
            .ToListAsync();
    }

    public async Task<ResumenLimitesPeriodoResponse> GetByPeriodoAsync(int anio, int mes)
    {
        var limites = await GetLimitesConGastosQuery()
            .Where(l => l.Anio == anio && l.Mes == mes)
            .OrderBy(l => l.CategoriaNombre)
            .ToListAsync();

        return new ResumenLimitesPeriodoResponse
        {
            Anio = anio,
            Mes = mes,
            TotalLimites = limites.Sum(l => l.MontoLimite),
            TotalGastado = limites.Sum(l => l.MontoGastado),
            CategoriasConLimite = limites.Count,
            CategoriasExcedidas = limites.Count(l => l.ExcedeLimite),
            Limites = limites
        };
    }

    public async Task<LimiteCategoriaResponse?> GetByIdAsync(int id)
    {
        return await GetLimitesConGastosQuery()
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<LimiteCategoriaResponse?> GetByCategoriaYPeriodoAsync(
        int categoriaId, int anio, int mes)
    {
        return await GetLimitesConGastosQuery()
            .FirstOrDefaultAsync(l => l.CategoriaId == categoriaId &&
                                      l.Anio == anio &&
                                      l.Mes == mes);
    }

    public async Task<HistoricoLimiteCategoriaResponse> GetHistoricoByCategoriaAsync(int categoriaId)
    {
        var categoria = await _context.Categorias
            .Where(c => c.Id == categoriaId && (c.EsPredefinida || c.UserId == UserId))
            .Select(c => new { c.Id, c.Nombre })
            .FirstOrDefaultAsync();

        if (categoria == null)
        {
            throw new ArgumentException("Categoría no encontrada o no válida");
        }

        var historico = await GetLimitesConGastosQuery()
            .Where(l => l.CategoriaId == categoriaId)
            .OrderByDescending(l => l.Anio)
            .ThenByDescending(l => l.Mes)
            .ToListAsync();

        return new HistoricoLimiteCategoriaResponse
        {
            CategoriaId = categoria.Id,
            CategoriaNombre = categoria.Nombre,
            Historico = historico
        };
    }

    public async Task<LimiteCategoriaResponse> CreateOrUpdateAsync(CreateLimiteCategoriaRequest request)
    {
        // Validar que la categoría existe y es válida para el usuario
        var categoriaValida = await _context.Categorias
            .AnyAsync(c => c.Id == request.CategoriaId &&
                          (c.Tipo == TipoCategoria.Gasto || c.Tipo == TipoCategoria.Ambos) &&
                          (c.EsPredefinida || c.UserId == UserId));

        if (!categoriaValida)
        {
            throw new ArgumentException("La categoría seleccionada no es válida o no es de tipo gasto");
        }

        // Buscar si ya existe un límite para este período/categoría
        var existente = await _context.LimitesCategorias
            .FirstOrDefaultAsync(l => l.UserId == UserId &&
                                      l.CategoriaId == request.CategoriaId &&
                                      l.Anio == request.Anio &&
                                      l.Mes == request.Mes);

        if (existente != null)
        {
            // Actualizar existente
            existente.MontoLimite = request.MontoLimite;
            existente.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
        else
        {
            // Crear nuevo
            existente = new LimiteCategoria
            {
                UserId = UserId,
                CategoriaId = request.CategoriaId,
                MontoLimite = request.MontoLimite,
                Anio = request.Anio,
                Mes = request.Mes,
                CreatedAt = DateTime.UtcNow
            };

            _context.LimitesCategorias.Add(existente);
            await _context.SaveChangesAsync();
        }

        // Retornar con el gasto calculado
        return (await GetLimitesConGastosQuery()
            .FirstAsync(l => l.Id == existente.Id));
    }

    public async Task<ResumenLimitesPeriodoResponse> CreateLoteAsync(CreateLimitesLoteRequest request)
    {
        foreach (var item in request.Limites)
        {
            await CreateOrUpdateAsync(new CreateLimiteCategoriaRequest
            {
                CategoriaId = item.CategoriaId,
                MontoLimite = item.MontoLimite,
                Anio = request.Anio,
                Mes = request.Mes
            });
        }

        return await GetByPeriodoAsync(request.Anio, request.Mes);
    }

    public async Task<LimiteCategoriaResponse?> UpdateAsync(int id, UpdateLimiteCategoriaRequest request)
    {
        var limite = await _context.LimitesCategorias
            .FirstOrDefaultAsync(l => l.Id == id && l.UserId == UserId);

        if (limite == null)
        {
            return null;
        }

        limite.MontoLimite = request.MontoLimite;
        limite.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await GetLimitesConGastosQuery()
            .FirstAsync(l => l.Id == id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var limite = await _context.LimitesCategorias
            .FirstOrDefaultAsync(l => l.Id == id && l.UserId == UserId);

        if (limite == null)
        {
            return false;
        }

        _context.LimitesCategorias.Remove(limite);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<ResumenLimitesPeriodoResponse> CopiarPeriodoAsync(
        int anioOrigen, int mesOrigen, int anioDestino, int mesDestino)
    {
        // Validar que no sea el mismo período
        if (anioOrigen == anioDestino && mesOrigen == mesDestino)
        {
            throw new ArgumentException("El período origen y destino no pueden ser iguales");
        }

        // Obtener límites del período origen
        var limitesOrigen = await _context.LimitesCategorias
            .Where(l => l.UserId == UserId && l.Anio == anioOrigen && l.Mes == mesOrigen)
            .ToListAsync();

        if (!limitesOrigen.Any())
        {
            throw new ArgumentException("No hay límites en el período origen");
        }

        // Crear límites en el período destino
        foreach (var limite in limitesOrigen)
        {
            await CreateOrUpdateAsync(new CreateLimiteCategoriaRequest
            {
                CategoriaId = limite.CategoriaId,
                MontoLimite = limite.MontoLimite,
                Anio = anioDestino,
                Mes = mesDestino
            });
        }

        return await GetByPeriodoAsync(anioDestino, mesDestino);
    }

    private IQueryable<LimiteCategoriaResponse> GetLimitesConGastosQuery()
    {
        return from l in _context.LimitesCategorias.Where(l => l.UserId == UserId)
               join c in _context.Categorias on l.CategoriaId equals c.Id
               let montoGastado = _context.Gastos
                   .Where(g => g.UserId == UserId &&
                               g.CategoriaId == l.CategoriaId &&
                               g.Fecha.Year == l.Anio &&
                               g.Fecha.Month == l.Mes)
                   .Sum(g => (decimal?)g.Monto) ?? 0
               select new LimiteCategoriaResponse
               {
                   Id = l.Id,
                   CategoriaId = l.CategoriaId,
                   CategoriaNombre = c.Nombre,
                   CategoriaColor = c.Color,
                   CategoriaIcono = c.Icono,
                   MontoLimite = l.MontoLimite,
                   Anio = l.Anio,
                   Mes = l.Mes,
                   MontoGastado = montoGastado,
                   CreatedAt = l.CreatedAt
               };
    }
}
