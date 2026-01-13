using Microsoft.Data.SqlClient;
using Npgsql;
using System.Data;

namespace PlanificadorGastos.API.Data;

public class DapperContext
{
    private readonly IConfiguration _configuration;
    private readonly string _connectionString;

    public DapperContext(IConfiguration configuration)
    {
        _configuration = configuration;
        _connectionString = _configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        UsePostgreSql = _configuration.GetValue<bool>("UsePostgreSql");
    }

    public bool UsePostgreSql { get; }

    public IDbConnection CreateConnection()
        => UsePostgreSql
            ? new NpgsqlConnection(_connectionString)
            : new SqlConnection(_connectionString);
}
