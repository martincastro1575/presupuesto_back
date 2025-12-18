using PlanificadorGastos.API.Models.DTOs.Auth;

namespace PlanificadorGastos.API.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request);
    Task RevokeTokenAsync(string refreshToken);
    Task<UserResponse> GetCurrentUserAsync();
}
