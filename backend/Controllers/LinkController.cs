using Amnecyne.LinkShort.DTOs;
using Amnecyne.LinkShort.Exceptions;
using Amnecyne.LinkShort.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Amnecyne.LinkShort.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LinkController(ShortLinkService linkService, ILogger<LinkController> logger) : ControllerBase
{
    private readonly ShortLinkService _linkService = linkService;
    private readonly ILogger<LinkController> _logger = logger;

    [AllowAnonymous]
    [HttpGet("{shortUrl}")]
    public async Task<IActionResult> GetShortLink([FromRoute] string shortUrl)
    {
        try
        {
            _logger.LogInformation("Resolving short link '{ShortUrl}'.", shortUrl);
            var link = await _linkService.GetShortLinkAsync(shortUrl);
            _logger.LogInformation("Redirecting short link '{ShortUrl}' to '{RedirectUrl}'.", shortUrl,
                link.RedirectUrl);
            return Redirect(link.RedirectUrl);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to resolve short link '{ShortUrl}'.", shortUrl);
            return NotFound(new { message = ex.Message });
        }
    }

    [AllowAnonymous]
    [HttpGet("resolve/{shortUrl}")]
    public async Task<IActionResult> ResolveShortLink([FromRoute] string shortUrl)
    {
        try
        {
            var link = await _linkService.GetShortLinkAsync(shortUrl);
            return Ok(link);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to resolve short link '{ShortUrl}'.", shortUrl);
            return NotFound(new { message = ex.Message });
        }
    }

    [AllowAnonymous]
    [EnableRateLimiting("public_access")]
    [HttpPost("create-random")]
    public async Task<IActionResult> CreateRandomShortLink([FromBody] IncomingShortLinkRequestDto requestDto)
    {
        try
        {
            var shortLink = await _linkService.CreateShortLinkAsync(requestDto, null);
            _logger.LogInformation("Anonymous short link created: '{ShortUrl}'.", shortLink.ShortUrl);
            return Ok(shortLink);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create anonymous short link.");
            return BadRequest(new { message = "An error occurred while creating the short link." });
        }
    }

    [Authorize]
    [HttpPost("create")]
    public async Task<IActionResult> CreateShortLink([FromBody] IncomingShortLinkRequestDto requestDto)
    {
        try
        {
            var username = User.Identity?.Name;
            var shortLink = await _linkService.CreateShortLinkAsync(requestDto, username);
            _logger.LogInformation("User '{Username}' created short link '{ShortUrl}'.", username, shortLink.ShortUrl);
            return Ok(shortLink);
        }
        catch (LinkTakenException ex1)
        {
            _logger.LogWarning(ex1, "Custom short link already taken for user '{Username}'.", User.Identity?.Name);
            return Conflict(new { message = ex1.Message });
        }
        catch (Exception ex2)
        {
            _logger.LogError(ex2, "Failed to create short link for user '{Username}'.", User.Identity?.Name);
            return BadRequest(new { message = "An error occurred while creating the short link." });
        }
    }

    [Authorize]
    [HttpGet("user-links")]
    public async Task<IActionResult> GetUserLinks()
    {
        var username = User.Identity?.Name;
        var userLinks = await _linkService.GetUserLinksAsync(username);
        _logger.LogInformation("Fetched {Count} short links for user '{Username}'.", userLinks.Count, username);
        return Ok(userLinks);
    }

    [Authorize]
    [HttpDelete("delete/{shortUrl}")]
    public async Task<IActionResult> DeleteShortLink(string shortUrl)
    {
        try
        {
            var username = User.Identity?.Name;
            await _linkService.DeleteShortLinkAsync(shortUrl, username);
            _logger.LogInformation("User '{Username}' deleted short link '{ShortUrl}'.", username, shortUrl);
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized delete attempt for short link '{ShortUrl}'.", shortUrl);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Delete failed because short link '{ShortUrl}' was not found.", shortUrl);
            return NotFound(new { message = ex.Message });
        }
    }
}