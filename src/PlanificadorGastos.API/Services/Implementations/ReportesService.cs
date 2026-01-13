using Dapper;
using PlanificadorGastos.API.Data;
using PlanificadorGastos.API.Models.DTOs.Reportes;
using PlanificadorGastos.API.Services.Interfaces;
using System.Globalization;

namespace PlanificadorGastos.API.Services.Implementations;

public class ReportesService : IReportesService
{
    private readonly DapperContext _dapperContext;
    private readonly ICurrentUserService _currentUserService;

    public ReportesService(DapperContext dapperContext, ICurrentUserService currentUserService)
    {
        _dapperContext = dapperContext;
        _currentUserService = currentUserService;
    }

    private int UserId => _currentUserService.UserId
        ?? throw new UnauthorizedAccessException("Usuario no autenticado");

    private bool UsePostgres => _dapperContext.UsePostgreSql;

    public async Task<ResumenMensualResponse> GetResumenMensualAsync(int? anio = null, int? mes = null)
    {
        var fecha = new DateOnly(anio ?? DateTime.Today.Year, mes ?? DateTime.Today.Month, 1);
        var fechaAnterior = fecha.AddMonths(-1);

        using var connection = _dapperContext.CreateConnection();

        var sql = UsePostgres ? GetResumenMensualSqlPostgres() : GetResumenMensualSqlServer();

        using var multi = await connection.QueryMultipleAsync(sql, new
        {
            UserId,
            Anio = fecha.Year,
            Mes = fecha.Month,
            AnioAnterior = fechaAnterior.Year,
            MesAnterior = fechaAnterior.Month
        });

        var resumenGastos = await multi.ReadFirstAsync<dynamic>();
        var resumenIngresos = await multi.ReadFirstAsync<dynamic>();
        var totalMesAnterior = await multi.ReadFirstAsync<decimal>();
        var categoriaMayor = await multi.ReadFirstOrDefaultAsync<GastoPorCategoriaResponse>();

        var totalGastado = (decimal)(resumenGastos.TotalGastado ?? resumenGastos.totalgastado ?? 0m);
        var totalIngresos = (decimal)(resumenIngresos.TotalIngresos ?? resumenIngresos.totalingresos ?? 0m);
        var cantidadIngresos = (int)(resumenIngresos.CantidadIngresos ?? resumenIngresos.cantidadingresos ?? 0);
        var cantidadGastos = (int)(resumenGastos.CantidadGastos ?? resumenGastos.cantidadgastos ?? 0);

        var diasEnMes = DateTime.DaysInMonth(fecha.Year, fecha.Month);
        var diasTranscurridos = fecha.Month == DateTime.Today.Month && fecha.Year == DateTime.Today.Year
            ? DateTime.Today.Day
            : diasEnMes;

        var diferencia = totalGastado - totalMesAnterior;
        var porcentajeCambio = totalMesAnterior > 0
            ? Math.Round(diferencia / totalMesAnterior * 100, 2)
            : 0;

        return new ResumenMensualResponse
        {
            Anio = fecha.Year,
            Mes = fecha.Month,
            TotalIngresos = totalIngresos,
            TotalGastado = totalGastado,
            CantidadIngresos = cantidadIngresos,
            CantidadGastos = cantidadGastos,
            PromedioGasto = cantidadGastos > 0 ? Math.Round(totalGastado / cantidadGastos, 2) : 0,
            PromedioGastoDiario = diasTranscurridos > 0 ? Math.Round(totalGastado / diasTranscurridos, 2) : 0,
            TotalMesAnterior = totalMesAnterior,
            DiferenciaMesAnterior = diferencia,
            PorcentajeCambio = porcentajeCambio,
            CategoriaMayorGasto = categoriaMayor
        };
    }

    private static string GetResumenMensualSqlServer() => @"
        -- Resumen mes actual (gastos)
        SELECT
            @Anio as Anio,
            @Mes as Mes,
            ISNULL(SUM(g.Monto), 0) as TotalGastado,
            COUNT(g.Id) as CantidadGastos
        FROM Gastos g
        WHERE g.UserId = @UserId
          AND YEAR(g.Fecha) = @Anio
          AND MONTH(g.Fecha) = @Mes;

        -- Resumen mes actual (ingresos)
        SELECT
            ISNULL(SUM(i.Monto), 0) as TotalIngresos,
            COUNT(i.Id) as CantidadIngresos
        FROM Ingresos i
        WHERE i.UserId = @UserId
          AND YEAR(i.Fecha) = @Anio
          AND MONTH(i.Fecha) = @Mes;

        -- Total mes anterior
        SELECT ISNULL(SUM(Monto), 0) as TotalMesAnterior
        FROM Gastos
        WHERE UserId = @UserId
          AND YEAR(Fecha) = @AnioAnterior
          AND MONTH(Fecha) = @MesAnterior;

        -- Categoría con mayor gasto
        SELECT TOP 1
            c.Id as CategoriaId,
            c.Nombre as CategoriaNombre,
            c.Icono as CategoriaIcono,
            c.Color as CategoriaColor,
            SUM(g.Monto) as TotalMonto,
            COUNT(g.Id) as CantidadGastos,
            0 as Porcentaje
        FROM Gastos g
        INNER JOIN Categorias c ON g.CategoriaId = c.Id
        WHERE g.UserId = @UserId
          AND YEAR(g.Fecha) = @Anio
          AND MONTH(g.Fecha) = @Mes
        GROUP BY c.Id, c.Nombre, c.Icono, c.Color
        ORDER BY SUM(g.Monto) DESC;
    ";

    private static string GetResumenMensualSqlPostgres() => @"
        -- Resumen mes actual (gastos)
        SELECT
            @Anio as Anio,
            @Mes as Mes,
            COALESCE(SUM(g.""Monto""), 0) as TotalGastado,
            COUNT(g.""Id"")::int as CantidadGastos
        FROM ""Gastos"" g
        WHERE g.""UserId"" = @UserId
          AND EXTRACT(YEAR FROM g.""Fecha"") = @Anio
          AND EXTRACT(MONTH FROM g.""Fecha"") = @Mes;

        -- Resumen mes actual (ingresos)
        SELECT
            COALESCE(SUM(i.""Monto""), 0) as TotalIngresos,
            COUNT(i.""Id"")::int as CantidadIngresos
        FROM ""Ingresos"" i
        WHERE i.""UserId"" = @UserId
          AND EXTRACT(YEAR FROM i.""Fecha"") = @Anio
          AND EXTRACT(MONTH FROM i.""Fecha"") = @Mes;

        -- Total mes anterior
        SELECT COALESCE(SUM(""Monto""), 0) as TotalMesAnterior
        FROM ""Gastos""
        WHERE ""UserId"" = @UserId
          AND EXTRACT(YEAR FROM ""Fecha"") = @AnioAnterior
          AND EXTRACT(MONTH FROM ""Fecha"") = @MesAnterior;

        -- Categoría con mayor gasto
        SELECT
            c.""Id"" as CategoriaId,
            c.""Nombre"" as CategoriaNombre,
            c.""Icono"" as CategoriaIcono,
            c.""Color"" as CategoriaColor,
            SUM(g.""Monto"") as TotalMonto,
            COUNT(g.""Id"")::int as CantidadGastos,
            0 as Porcentaje
        FROM ""Gastos"" g
        INNER JOIN ""Categorias"" c ON g.""CategoriaId"" = c.""Id""
        WHERE g.""UserId"" = @UserId
          AND EXTRACT(YEAR FROM g.""Fecha"") = @Anio
          AND EXTRACT(MONTH FROM g.""Fecha"") = @Mes
        GROUP BY c.""Id"", c.""Nombre"", c.""Icono"", c.""Color""
        ORDER BY SUM(g.""Monto"") DESC
        LIMIT 1;
    ";

    public async Task<IEnumerable<GastoPorCategoriaResponse>> GetGastosPorCategoriaAsync(int? anio = null, int? mes = null)
    {
        var fecha = new DateOnly(anio ?? DateTime.Today.Year, mes ?? DateTime.Today.Month, 1);

        using var connection = _dapperContext.CreateConnection();

        var sql = UsePostgres ? GetGastosPorCategoriaSqlPostgres() : GetGastosPorCategoriaSqlServer();

        var result = await connection.QueryAsync<GastoPorCategoriaResponse>(sql, new
        {
            UserId,
            Anio = fecha.Year,
            Mes = fecha.Month
        });

        return result;
    }

    private static string GetGastosPorCategoriaSqlServer() => @"
        WITH TotalMes AS (
            SELECT ISNULL(SUM(Monto), 0) as Total
            FROM Gastos
            WHERE UserId = @UserId
              AND YEAR(Fecha) = @Anio
              AND MONTH(Fecha) = @Mes
        )
        SELECT
            c.Id as CategoriaId,
            c.Nombre as CategoriaNombre,
            c.Icono as CategoriaIcono,
            c.Color as CategoriaColor,
            ISNULL(SUM(g.Monto), 0) as TotalMonto,
            COUNT(g.Id) as CantidadGastos,
            CASE WHEN t.Total > 0
                 THEN ROUND(ISNULL(SUM(g.Monto), 0) / t.Total * 100, 2)
                 ELSE 0
            END as Porcentaje
        FROM Categorias c
        CROSS JOIN TotalMes t
        LEFT JOIN Gastos g ON g.CategoriaId = c.Id
            AND g.UserId = @UserId
            AND YEAR(g.Fecha) = @Anio
            AND MONTH(g.Fecha) = @Mes
        WHERE (c.EsPredefinida = 1 OR c.UserId = @UserId)
          AND c.Activa = 1
        GROUP BY c.Id, c.Nombre, c.Icono, c.Color, t.Total
        HAVING ISNULL(SUM(g.Monto), 0) > 0
        ORDER BY TotalMonto DESC;
    ";

    private static string GetGastosPorCategoriaSqlPostgres() => @"
        WITH TotalMes AS (
            SELECT COALESCE(SUM(""Monto""), 0) as Total
            FROM ""Gastos""
            WHERE ""UserId"" = @UserId
              AND EXTRACT(YEAR FROM ""Fecha"") = @Anio
              AND EXTRACT(MONTH FROM ""Fecha"") = @Mes
        )
        SELECT
            c.""Id"" as CategoriaId,
            c.""Nombre"" as CategoriaNombre,
            c.""Icono"" as CategoriaIcono,
            c.""Color"" as CategoriaColor,
            COALESCE(SUM(g.""Monto""), 0) as TotalMonto,
            COUNT(g.""Id"")::int as CantidadGastos,
            CASE WHEN t.Total > 0
                 THEN ROUND(COALESCE(SUM(g.""Monto""), 0) / t.Total * 100, 2)
                 ELSE 0
            END as Porcentaje
        FROM ""Categorias"" c
        CROSS JOIN TotalMes t
        LEFT JOIN ""Gastos"" g ON g.""CategoriaId"" = c.""Id""
            AND g.""UserId"" = @UserId
            AND EXTRACT(YEAR FROM g.""Fecha"") = @Anio
            AND EXTRACT(MONTH FROM g.""Fecha"") = @Mes
        WHERE (c.""EsPredefinida"" = TRUE OR c.""UserId"" = @UserId)
          AND c.""Activa"" = TRUE
        GROUP BY c.""Id"", c.""Nombre"", c.""Icono"", c.""Color"", t.Total
        HAVING COALESCE(SUM(g.""Monto""), 0) > 0
        ORDER BY TotalMonto DESC;
    ";

    public async Task<IEnumerable<EvolucionMensualResponse>> GetEvolucionMensualAsync(int meses = 6)
    {
        using var connection = _dapperContext.CreateConnection();

        var sql = UsePostgres ? GetEvolucionMensualSqlPostgres() : GetEvolucionMensualSqlServer();

        var result = await connection.QueryAsync<EvolucionMensualResponse>(sql, new { UserId, Meses = meses });

        // Agregar nombre del mes
        var cultura = new CultureInfo("es-AR");
        return result.Select(e => e with
        {
            MesNombre = cultura.DateTimeFormat.GetMonthName(e.Mes)
        });
    }

    private static string GetEvolucionMensualSqlServer() => @"
        WITH Meses AS (
            SELECT TOP (@Meses)
                YEAR(DATEADD(MONTH, -ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) + 1, GETDATE())) as Anio,
                MONTH(DATEADD(MONTH, -ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) + 1, GETDATE())) as Mes
            FROM sys.objects
        )
        SELECT
            m.Anio,
            m.Mes,
            '' as MesNombre,
            ISNULL((SELECT SUM(Monto) FROM Gastos WHERE UserId = @UserId AND YEAR(Fecha) = m.Anio AND MONTH(Fecha) = m.Mes), 0) as TotalGastado,
            ISNULL((SELECT SUM(Monto) FROM Ingresos WHERE UserId = @UserId AND YEAR(Fecha) = m.Anio AND MONTH(Fecha) = m.Mes), 0) as TotalIngresos,
            ISNULL((SELECT COUNT(*) FROM Gastos WHERE UserId = @UserId AND YEAR(Fecha) = m.Anio AND MONTH(Fecha) = m.Mes), 0) as CantidadGastos,
            ISNULL((SELECT COUNT(*) FROM Ingresos WHERE UserId = @UserId AND YEAR(Fecha) = m.Anio AND MONTH(Fecha) = m.Mes), 0) as CantidadIngresos
        FROM Meses m
        ORDER BY m.Anio, m.Mes;
    ";

    private static string GetEvolucionMensualSqlPostgres() => @"
        WITH Meses AS (
            SELECT
                EXTRACT(YEAR FROM (CURRENT_DATE - (s.n || ' months')::interval))::int as Anio,
                EXTRACT(MONTH FROM (CURRENT_DATE - (s.n || ' months')::interval))::int as Mes
            FROM generate_series(0, @Meses - 1) AS s(n)
        )
        SELECT
            m.Anio,
            m.Mes,
            '' as MesNombre,
            COALESCE((SELECT SUM(""Monto"") FROM ""Gastos"" WHERE ""UserId"" = @UserId AND EXTRACT(YEAR FROM ""Fecha"") = m.Anio AND EXTRACT(MONTH FROM ""Fecha"") = m.Mes), 0) as TotalGastado,
            COALESCE((SELECT SUM(""Monto"") FROM ""Ingresos"" WHERE ""UserId"" = @UserId AND EXTRACT(YEAR FROM ""Fecha"") = m.Anio AND EXTRACT(MONTH FROM ""Fecha"") = m.Mes), 0) as TotalIngresos,
            COALESCE((SELECT COUNT(*)::int FROM ""Gastos"" WHERE ""UserId"" = @UserId AND EXTRACT(YEAR FROM ""Fecha"") = m.Anio AND EXTRACT(MONTH FROM ""Fecha"") = m.Mes), 0) as CantidadGastos,
            COALESCE((SELECT COUNT(*)::int FROM ""Ingresos"" WHERE ""UserId"" = @UserId AND EXTRACT(YEAR FROM ""Fecha"") = m.Anio AND EXTRACT(MONTH FROM ""Fecha"") = m.Mes), 0) as CantidadIngresos
        FROM Meses m
        ORDER BY m.Anio, m.Mes;
    ";

    public async Task<IEnumerable<IngresoPorCategoriaResponse>> GetIngresosPorCategoriaAsync(int? anio = null, int? mes = null)
    {
        var fecha = new DateOnly(anio ?? DateTime.Today.Year, mes ?? DateTime.Today.Month, 1);

        using var connection = _dapperContext.CreateConnection();

        var sql = UsePostgres ? GetIngresosPorCategoriaSqlPostgres() : GetIngresosPorCategoriaSqlServer();

        var result = await connection.QueryAsync<IngresoPorCategoriaResponse>(sql, new
        {
            UserId,
            Anio = fecha.Year,
            Mes = fecha.Month
        });

        return result;
    }

    private static string GetIngresosPorCategoriaSqlServer() => @"
        WITH TotalMes AS (
            SELECT ISNULL(SUM(Monto), 0) as Total
            FROM Ingresos
            WHERE UserId = @UserId
              AND YEAR(Fecha) = @Anio
              AND MONTH(Fecha) = @Mes
        )
        SELECT
            c.Id as CategoriaId,
            c.Nombre as CategoriaNombre,
            c.Icono as CategoriaIcono,
            c.Color as CategoriaColor,
            ISNULL(SUM(i.Monto), 0) as TotalMonto,
            COUNT(i.Id) as CantidadIngresos,
            CASE WHEN t.Total > 0
                 THEN ROUND(ISNULL(SUM(i.Monto), 0) / t.Total * 100, 2)
                 ELSE 0
            END as Porcentaje
        FROM Categorias c
        CROSS JOIN TotalMes t
        LEFT JOIN Ingresos i ON i.CategoriaId = c.Id
            AND i.UserId = @UserId
            AND YEAR(i.Fecha) = @Anio
            AND MONTH(i.Fecha) = @Mes
        WHERE (c.EsPredefinida = 1 OR c.UserId = @UserId)
          AND c.Activa = 1
          AND (c.Tipo = 2 OR c.Tipo = 3)
        GROUP BY c.Id, c.Nombre, c.Icono, c.Color, t.Total
        HAVING ISNULL(SUM(i.Monto), 0) > 0
        ORDER BY TotalMonto DESC;
    ";

    private static string GetIngresosPorCategoriaSqlPostgres() => @"
        WITH TotalMes AS (
            SELECT COALESCE(SUM(""Monto""), 0) as Total
            FROM ""Ingresos""
            WHERE ""UserId"" = @UserId
              AND EXTRACT(YEAR FROM ""Fecha"") = @Anio
              AND EXTRACT(MONTH FROM ""Fecha"") = @Mes
        )
        SELECT
            c.""Id"" as CategoriaId,
            c.""Nombre"" as CategoriaNombre,
            c.""Icono"" as CategoriaIcono,
            c.""Color"" as CategoriaColor,
            COALESCE(SUM(i.""Monto""), 0) as TotalMonto,
            COUNT(i.""Id"")::int as CantidadIngresos,
            CASE WHEN t.Total > 0
                 THEN ROUND(COALESCE(SUM(i.""Monto""), 0) / t.Total * 100, 2)
                 ELSE 0
            END as Porcentaje
        FROM ""Categorias"" c
        CROSS JOIN TotalMes t
        LEFT JOIN ""Ingresos"" i ON i.""CategoriaId"" = c.""Id""
            AND i.""UserId"" = @UserId
            AND EXTRACT(YEAR FROM i.""Fecha"") = @Anio
            AND EXTRACT(MONTH FROM i.""Fecha"") = @Mes
        WHERE (c.""EsPredefinida"" = TRUE OR c.""UserId"" = @UserId)
          AND c.""Activa"" = TRUE
          AND (c.""Tipo"" = 2 OR c.""Tipo"" = 3)
        GROUP BY c.""Id"", c.""Nombre"", c.""Icono"", c.""Color"", t.Total
        HAVING COALESCE(SUM(i.""Monto""), 0) > 0
        ORDER BY TotalMonto DESC;
    ";

    public async Task<ComparativoMensualResponse> GetComparativoMensualAsync()
    {
        var mesActual = await GetResumenMensualAsync();
        var mesAnterior = await GetResumenMensualAsync(
            DateTime.Today.AddMonths(-1).Year,
            DateTime.Today.AddMonths(-1).Month);

        using var connection = _dapperContext.CreateConnection();

        var sql = UsePostgres ? GetComparativoMensualSqlPostgres() : GetComparativoMensualSqlServer();

        var fechaAnterior = DateTime.Today.AddMonths(-1);
        var comparativo = await connection.QueryAsync<ComparativoCategoriaResponse>(sql, new
        {
            UserId,
            AnioActual = DateTime.Today.Year,
            MesActual = DateTime.Today.Month,
            AnioAnterior = fechaAnterior.Year,
            MesAnterior = fechaAnterior.Month
        });

        // Calcular diferencias
        var comparativoConDiferencias = comparativo.Select(c =>
        {
            var diferencia = c.MontoMesActual - c.MontoMesAnterior;
            var porcentaje = c.MontoMesAnterior > 0
                ? Math.Round(diferencia / c.MontoMesAnterior * 100, 2)
                : 0;

            return c with
            {
                Diferencia = diferencia,
                PorcentajeCambio = porcentaje
            };
        }).ToList();

        return new ComparativoMensualResponse
        {
            MesActual = mesActual,
            MesAnterior = mesAnterior,
            ComparativoPorCategoria = comparativoConDiferencias
        };
    }

    private static string GetComparativoMensualSqlServer() => @"
        SELECT
            c.Id as CategoriaId,
            c.Nombre as CategoriaNombre,
            c.Color as CategoriaColor,
            ISNULL(SUM(CASE WHEN YEAR(g.Fecha) = @AnioActual AND MONTH(g.Fecha) = @MesActual THEN g.Monto END), 0) as MontoMesActual,
            ISNULL(SUM(CASE WHEN YEAR(g.Fecha) = @AnioAnterior AND MONTH(g.Fecha) = @MesAnterior THEN g.Monto END), 0) as MontoMesAnterior,
            0 as Diferencia,
            0 as PorcentajeCambio
        FROM Categorias c
        LEFT JOIN Gastos g ON g.CategoriaId = c.Id AND g.UserId = @UserId
        WHERE (c.EsPredefinida = 1 OR c.UserId = @UserId)
        GROUP BY c.Id, c.Nombre, c.Color
        HAVING ISNULL(SUM(CASE WHEN YEAR(g.Fecha) = @AnioActual AND MONTH(g.Fecha) = @MesActual THEN g.Monto END), 0) > 0
            OR ISNULL(SUM(CASE WHEN YEAR(g.Fecha) = @AnioAnterior AND MONTH(g.Fecha) = @MesAnterior THEN g.Monto END), 0) > 0
        ORDER BY MontoMesActual DESC;
    ";

    private static string GetComparativoMensualSqlPostgres() => @"
        SELECT
            c.""Id"" as CategoriaId,
            c.""Nombre"" as CategoriaNombre,
            c.""Color"" as CategoriaColor,
            COALESCE(SUM(CASE WHEN EXTRACT(YEAR FROM g.""Fecha"") = @AnioActual AND EXTRACT(MONTH FROM g.""Fecha"") = @MesActual THEN g.""Monto"" END), 0) as MontoMesActual,
            COALESCE(SUM(CASE WHEN EXTRACT(YEAR FROM g.""Fecha"") = @AnioAnterior AND EXTRACT(MONTH FROM g.""Fecha"") = @MesAnterior THEN g.""Monto"" END), 0) as MontoMesAnterior,
            0 as Diferencia,
            0 as PorcentajeCambio
        FROM ""Categorias"" c
        LEFT JOIN ""Gastos"" g ON g.""CategoriaId"" = c.""Id"" AND g.""UserId"" = @UserId
        WHERE (c.""EsPredefinida"" = TRUE OR c.""UserId"" = @UserId)
        GROUP BY c.""Id"", c.""Nombre"", c.""Color""
        HAVING COALESCE(SUM(CASE WHEN EXTRACT(YEAR FROM g.""Fecha"") = @AnioActual AND EXTRACT(MONTH FROM g.""Fecha"") = @MesActual THEN g.""Monto"" END), 0) > 0
            OR COALESCE(SUM(CASE WHEN EXTRACT(YEAR FROM g.""Fecha"") = @AnioAnterior AND EXTRACT(MONTH FROM g.""Fecha"") = @MesAnterior THEN g.""Monto"" END), 0) > 0
        ORDER BY MontoMesActual DESC;
    ";
}
