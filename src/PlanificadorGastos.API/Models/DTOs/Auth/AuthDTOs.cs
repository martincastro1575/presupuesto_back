using System.ComponentModel.DataAnnotations;

namespace PlanificadorGastos.API.Models.DTOs.Auth;

public record RegisterRequest
{
    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
    public string Nombre { get; init; } = string.Empty;

    [Required(ErrorMessage = "El email es requerido")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es requerida")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
    public string Password { get; init; } = string.Empty;

    [Required(ErrorMessage = "La confirmación de contraseña es requerida")]
    [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden")]
    public string ConfirmPassword { get; init; } = string.Empty;
}

public record LoginRequest
{
    [Required(ErrorMessage = "El email es requerido")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es requerida")]
    public string Password { get; init; } = string.Empty;
}

public record RefreshTokenRequest
{
    [Required(ErrorMessage = "El token es requerido")]
    public string Token { get; init; } = string.Empty;

    [Required(ErrorMessage = "El refresh token es requerido")]
    public string RefreshToken { get; init; } = string.Empty;
}

public record AuthResponse
{
    public int UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public string Token { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public DateTime TokenExpiration { get; init; }
}

public record UserResponse
{
    public int Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? LastLogin { get; init; }
}
