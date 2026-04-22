namespace Amnecyne.LinkShort.DTOs;

public class IncomingShortLinkRequestDto
{
    public string? Name { get; set; }
    public required string RedirectUrl { get; set; }
    public string? CustomShortLink { get; set; } = null;
    public string? Description { get; set; } = null;
}