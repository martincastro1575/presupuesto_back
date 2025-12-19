using Microsoft.Data.SqlClient;
using Npgsql;
using System.Data;

namespace PlanificadorGastos.API.Data;

public class DapperContext
{
    private readonly IConfiguration _configuration;
    private readonly string _connectionString;
    private readonly bool _usePostgreSql;

    public DapperContext(IConfiguration configuration)
    {
        _configuration = configuration;
        _connectionString = _configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        _usePostgreSql = _configuration.GetValue<bool>("UsePostgreSql");
    }

    public IDbConnection CreateConnection()
        => _usePostgreSql
            ? new NpgsqlConnection(_connectionString)
            : new SqlConnection(_connectionString);
}
