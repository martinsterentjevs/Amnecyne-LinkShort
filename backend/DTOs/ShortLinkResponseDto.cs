namespace Amnecyne.LinkShort.DTOs;

public class ShortLinkResponseDto
{
    public required string Name { get; set; }
    public required string ShortUrl { get; set; }
    public int? TTL { get; set; } = 15;
}