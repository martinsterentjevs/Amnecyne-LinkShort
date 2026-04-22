using System.Text.RegularExpressions;
using Amnecyne.LinkShort.DTOs;
using Amnecyne.LinkShort.Exceptions;
using Amnecyne.LinkShort.Helpers;
using Amnecyne.LinkShort.Models;

namespace Amnecyne.LinkShort.Services;

public class ShortLinkService(DBStorageService dbService, ILogger<ShortLinkService> logger)
{
    private static readonly Regex CustomShortLinkRegex = new("^[a-zA-Z0-9_-]{3,32}$", RegexOptions.Compiled);

    private DBStorageService DbService { get; } = dbService;
    private ILogger<ShortLinkService> Logger { get; } = logger;

    public async Task<ShortLink> CreateShortLinkAsync(IncomingShortLinkRequestDto requestDto, string? username)
    {
        ValidateCreateRequest(requestDto);

        if (requestDto.CustomShortLink != null && await DbService.IsShortUrlTakenAsync(requestDto.CustomShortLink))
        {
            Logger.LogError("Custom short link '{CustomShortLink}' is already taken.", requestDto.CustomShortLink);
            throw new LinkTakenException("Custom short link is already taken.");
        }

        var shortUrl = requestDto.CustomShortLink ?? ShortLinkGenerator.GenerateShortUrl();
        var shortLink = new ShortLink
        {
            Name = requestDto.Name,
            UserId = username ?? null,
            RedirectUrl = requestDto.RedirectUrl,
            ShortUrl = shortUrl,
            Description = requestDto.Description
        };
        await DbService.AddShortLinkAsync(shortLink);
        Logger.LogInformation("Short link '{ShortUrl}' created for user '{Username}'.", shortLink.ShortUrl,
            username ?? "anonymous");
        var response = new ShortLinkResponseDto
        {
            Name = requestDto.Name ?? "",
            ShortUrl = shortLink.ShortUrl
        };
        return shortLink;
    }

    public async Task<List<ShortLink>> GetUserLinksAsync(string? username)
    {
        if (username == null) throw new ArgumentNullException(nameof(username));
        var links = await DbService.GetAllShortLinksAsync(username);
        Logger.LogInformation("Retrieved {Count} links for user '{Username}'.", links.Count, username);
        return links;
    }

    internal async Task DeleteShortLinkAsync(string shortUrl, string? username)
    {
        var shortLink = await DbService.GetShortLinkByIdAsync(shortUrl);
        if (shortLink == null)
        {
            Logger.LogWarning("Delete requested for missing short link '{ShortUrl}'.", shortUrl);
            throw new Exception("Short link not found.");
        }

        if (shortLink.UserId != username)
        {
            Logger.LogWarning("Unauthorized delete attempt on '{ShortUrl}' by '{Username}'.", shortUrl, username);
            throw new UnauthorizedAccessException("You are not authorized to delete this short link.");
        }

        await DbService.RemoveShortLinkAsync(shortLink);
        Logger.LogInformation("Short link '{ShortUrl}' deleted by user '{Username}'.", shortUrl, username);
    }

    public async Task<ShortLink> GetShortLinkAsync(string shortUrl)
    {
        var shortLink = await DbService.GetShortLinkByIdAsync(shortUrl);
        if (shortLink == null)
        {
            Logger.LogWarning("Short link '{ShortUrl}' not found during redirect.", shortUrl);
            throw new Exception("Short link not found.");
        }

        Logger.LogInformation("Resolved short link '{ShortUrl}' to '{RedirectUrl}'.", shortUrl, shortLink.RedirectUrl);
        return shortLink;
    }

    private void ValidateCreateRequest(IncomingShortLinkRequestDto requestDto)
    {
        if (string.IsNullOrWhiteSpace(requestDto.RedirectUrl)
            || !Uri.TryCreate(requestDto.RedirectUrl, UriKind.Absolute, out var redirectUri)
            || (redirectUri.Scheme != Uri.UriSchemeHttp && redirectUri.Scheme != Uri.UriSchemeHttps))
        {
            Logger.LogWarning("Invalid redirect url provided: '{RedirectUrl}'.", requestDto.RedirectUrl);
            throw new ArgumentException("RedirectUrl must be a valid absolute HTTP/HTTPS URL.");
        }

        if (!string.IsNullOrWhiteSpace(requestDto.CustomShortLink)
            && !CustomShortLinkRegex.IsMatch(requestDto.CustomShortLink))
        {
            Logger.LogWarning("Invalid custom short link format provided: '{CustomShortLink}'.",
                requestDto.CustomShortLink);
            throw new ArgumentException(
                "CustomShortLink must be 3-32 chars and only contain letters, numbers, '-' or '_'.");
        }
    }
}