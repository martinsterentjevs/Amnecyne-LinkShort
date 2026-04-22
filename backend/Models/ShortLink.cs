namespace Amnecyne.LinkShort.Models;

public class ShortLink
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? Name { get; set; }
    public string? UserId { get; set; }
    public required string RedirectUrl { get; set; }
    public required string ShortUrl { get; set; }
    public string? Description { get; set; }
}