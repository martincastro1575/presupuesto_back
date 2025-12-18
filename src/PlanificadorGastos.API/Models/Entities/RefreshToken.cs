namespace PlanificadorGastos.API.Models.Entities;

public class RefreshToken
{
    public int Id { get; set; }
    
    public int UserId { get; set; }
    
    public string Token { get; set; } = string.Empty;
    
    public DateTime ExpiresAt { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? RevokedAt { get; set; }
    
    public string? ReplacedByToken { get; set; }
    
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    
    public bool IsRevoked => RevokedAt != null;
    
    public bool IsActive => !IsRevoked && !IsExpired;
    
    // Navigation properties
    public virtual Usuario Usuario { get; set; } = null!;
}
