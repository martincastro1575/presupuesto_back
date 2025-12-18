using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PlanificadorGastos.API.Data;
using PlanificadorGastos.API.Infrastructure.Authentication;
using PlanificadorGastos.API.Models.DTOs.Auth;
using PlanificadorGastos.API.Models.Entities;
using PlanificadorGastos.API.Services.Interfaces;
using System.Security.Claims;

namespace PlanificadorGastos.API.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly UserManager<Usuario> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<Usuario> userManager,
        ApplicationDbContext context,
        IJwtService jwtService,
        ICurrentUserService currentUserService,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _context = context;
        _jwtService = jwtService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        // Verificar si el email ya existe
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException("El email ya está registrado");
        }

        var usuario = new Usuario
        {
            UserName = request.Email,
            Email = request.Email,
            Nombre = request.Nombre,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(usuario, request.Password);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            throw new InvalidOperationException($"Error al crear usuario: {string.Join(", ", errors)}");
        }

        _logger.LogInformation("Usuario registrado: {Email}", request.Email);

        return await GenerateAuthResponseAsync(usuario);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var usuario = await _userManager.FindByEmailAsync(request.Email);

        if (usuario == null || !await _userManager.CheckPasswordAsync(usuario, request.Password))
        {
            throw new UnauthorizedAccessException("Credenciales inválidas");
        }

        // Actualizar último login
        usuario.LastLogin = DateTime.UtcNow;
        await _userManager.UpdateAsync(usuario);

        _logger.LogInformation("Usuario logueado: {Email}", request.Email);

        return await GenerateAuthResponseAsync(usuario);
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var principal = _jwtService.GetPrincipalFromExpiredToken(request.Token);
        if (principal == null)
        {
            throw new UnauthorizedAccessException("Token inválido");
        }

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Token inválido");
        }

        var usuario = await _userManager.FindByIdAsync(userId.ToString());
        if (usuario == null)
        {
            throw new UnauthorizedAccessException("Usuario no encontrado");
        }

        // Buscar refresh token activo
        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.UserId == userId && 
                                       rt.Token == request.RefreshToken && 
                                       rt.RevokedAt == null);

        if (refreshToken == null || !refreshToken.IsActive)
        {
            throw new UnauthorizedAccessException("Refresh token inválido o expirado");
        }

        // Revocar el token actual
        refreshToken.RevokedAt = DateTime.UtcNow;

        // Generar nuevos tokens
        var newRefreshToken = CreateRefreshToken(usuario.Id);
        refreshToken.ReplacedByToken = newRefreshToken.Token;

        _context.RefreshTokens.Add(newRefreshToken);
        await _context.SaveChangesAsync();

        return new AuthResponse
        {
            UserId = usuario.Id,
            Email = usuario.Email ?? string.Empty,
            Nombre = usuario.Nombre,
            Token = _jwtService.GenerateAccessToken(usuario),
            RefreshToken = newRefreshToken.Token,
            TokenExpiration = _jwtService.GetAccessTokenExpiration()
        };
    }

    public async Task RevokeTokenAsync(string refreshToken)
    {
        var token = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken && rt.RevokedAt == null);

        if (token != null)
        {
            token.RevokedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<UserResponse> GetCurrentUserAsync()
    {
        var userId = _currentUserService.UserId 
            ?? throw new UnauthorizedAccessException("Usuario no autenticado");

        var usuario = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new KeyNotFoundException("Usuario no encontrado");

        return new UserResponse
        {
            Id = usuario.Id,
            Email = usuario.Email ?? string.Empty,
            Nombre = usuario.Nombre,
            CreatedAt = usuario.CreatedAt,
            LastLogin = usuario.LastLogin
        };
    }

    private async Task<AuthResponse> GenerateAuthResponseAsync(Usuario usuario)
    {
        // Limpiar refresh tokens expirados/revocados del usuario
        var oldTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == usuario.Id && (rt.RevokedAt != null || rt.ExpiresAt < DateTime.UtcNow))
            .ToListAsync();

        _context.RefreshTokens.RemoveRange(oldTokens);

        // Crear nuevo refresh token
        var refreshToken = CreateRefreshToken(usuario.Id);
        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        return new AuthResponse
        {
            UserId = usuario.Id,
            Email = usuario.Email ?? string.Empty,
            Nombre = usuario.Nombre,
            Token = _jwtService.GenerateAccessToken(usuario),
            RefreshToken = refreshToken.Token,
            TokenExpiration = _jwtService.GetAccessTokenExpiration()
        };
    }

    private RefreshToken CreateRefreshToken(int userId)
    {
        return new RefreshToken
        {
            UserId = userId,
            Token = _jwtService.GenerateRefreshToken(),
            ExpiresAt = _jwtService.GetRefreshTokenExpiration(),
            CreatedAt = DateTime.UtcNow
        };
    }
}
