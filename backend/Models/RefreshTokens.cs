namespace Amnecyne.LinkShort.Models;

public class RefreshTokens
{
    public int Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime Expires { get; set; }
    public bool IsRevoked { get; set; }
}