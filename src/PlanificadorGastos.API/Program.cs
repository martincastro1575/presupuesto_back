using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PlanificadorGastos.API.Data;
using PlanificadorGastos.API.Infrastructure.Authentication;
using PlanificadorGastos.API.Infrastructure.Extensions;
using PlanificadorGastos.API.Middleware;
using PlanificadorGastos.API.Models.Entities;
using Serilog;
using StackExchange.Profiling;
using System.IO.Compression;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// SERILOG CONFIGURATION
// ============================================
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

// ============================================
// DATABASE CONFIGURATION
// ============================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var usePostgreSql = builder.Configuration.GetValue<bool>("UsePostgreSql");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (usePostgreSql)
    {
        options.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorCodesToAdd: null);
        });
    }
    else
    {
        options.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null);
        });
    }

    options.ConfigureWarnings(warnings =>
        warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
});

// Dapper - Registrar connection string para inyección
builder.Services.AddScoped<DapperContext>();

// ============================================
// IDENTITY CONFIGURATION
// ============================================
builder.Services.AddIdentity<Usuario, IdentityRole<int>>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;

    // User settings
    options.User.RequireUniqueEmail = true;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ============================================
// JWT AUTHENTICATION
// ============================================
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()!;
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// ============================================
// APPLICATION SERVICES
// ============================================
builder.Services.AddApplicationServices();

// ============================================
// AUTOMAPPER
// ============================================
builder.Services.AddAutoMapper(typeof(Program).Assembly);

// ============================================
// CORS
// ============================================
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ============================================
// RESPONSE COMPRESSION
// ============================================
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.SmallestSize;
});

// ============================================
// MINIPROFILER
// ============================================
builder.Services.AddMiniProfiler(options =>
{
    options.RouteBasePath = "/profiler";
    options.ColorScheme = StackExchange.Profiling.ColorScheme.Dark;
    options.EnableServerTimingHeader = true;
}).AddEntityFramework();

// ============================================
// CONTROLLERS
// ============================================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// ============================================
// SWAGGER / OPENAPI
// ============================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Planificador de Gastos API",
        Version = "v1",
        Description = "API REST para gestión de gastos personales",
        Contact = new OpenApiContact
        {
            Name = "Martin",
            Email = "martin@example.com"
        }
    });

    // JWT Authentication en Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese el token JWT en el formato: Bearer {token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ============================================
// BUILD APPLICATION
// ============================================
var app = builder.Build();

// ============================================
// MIDDLEWARE PIPELINE
// ============================================

// Exception handling (debe ir primero)
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Response compression
app.UseResponseCompression();

// Serilog request logging
app.UseSerilogRequestLogging();

// Development-specific middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Planificador de Gastos API v1");
        options.RoutePrefix = "swagger";
    });

    app.UseMiniProfiler();
}

// HTTPS redirection
app.UseHttpsRedirection();

// CORS
app.UseCors("DefaultPolicy");

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

// ============================================
// DATABASE INITIALIZATION
// ============================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();

        // En producción con PostgreSQL, crear la BD si no existe
        if (usePostgreSql)
        {
            await context.Database.EnsureCreatedAsync();
        }
        else
        {
            // En desarrollo con SQL Server, usar migraciones
            await context.Database.MigrateAsync();
        }

        Log.Information("Base de datos inicializada correctamente");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error al inicializar la base de datos");
    }
}

// ============================================
// RUN APPLICATION
// ============================================
Log.Information("Iniciando Planificador de Gastos API...");

try
{
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "La aplicación terminó inesperadamente");
}
finally
{
    Log.CloseAndFlush();
}
