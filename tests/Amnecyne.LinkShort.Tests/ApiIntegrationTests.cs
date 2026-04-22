using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Amnecyne.LinkShort.Data;
using Amnecyne.LinkShort.DTOs;
using Amnecyne.LinkShort.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Amnecyne.LinkShort.Tests;

public class ApiIntegrationTests : IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    public ApiIntegrationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    public async Task InitializeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Health_ReturnsOk()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Register_Then_Login_ReturnsTokens()
    {
        var unique = Guid.NewGuid().ToString("N")[..8];
        var register = new RegisterModel
        {
            Username = $"user-{unique}",
            Full_Name = "Integration Test User",
            Email = $"{unique}@test.local",
            Password = "TestPassword123!"
        };

        var registerResponse = await _client.PostAsJsonAsync("/Auth/Register", register);
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        var registerTokens = await registerResponse.Content.ReadFromJsonAsync<TokenResponse>();
        Assert.False(string.IsNullOrWhiteSpace(registerTokens?.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(registerTokens?.RefreshToken));

        var login = new LoginModel
        {
            User = register.Username,
            Password = register.Password
        };

        var loginResponse = await _client.PostAsJsonAsync("/Auth/Login", login);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var loginTokens = await loginResponse.Content.ReadFromJsonAsync<TokenResponse>();
        Assert.False(string.IsNullOrWhiteSpace(loginTokens?.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(loginTokens?.RefreshToken));
    }

    [Fact]
    public async Task CreateRandomShortLink_InvalidRedirectUrl_ReturnsBadRequest()
    {
        var request = new IncomingShortLinkRequestDto
        {
            Name = "bad-link",
            RedirectUrl = "not-a-url"
        };

        var response = await _client.PostAsJsonAsync("/api/Link/create-random", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateRandomShortLink_EmptyRedirectUrl_ReturnsBadRequest()
    {
        var request = new IncomingShortLinkRequestDto
        {
            Name = "empty-redirect-random",
            RedirectUrl = string.Empty
        };

        var response = await _client.PostAsJsonAsync("/api/Link/create-random", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateRandomShortLink_Then_Resolve_ReturnsRedirect()
    {
        var shortCode = $"go-{Guid.NewGuid().ToString("N")[..8]}";
        const string targetUrl = "https://example.org/docs";

        var request = new IncomingShortLinkRequestDto
        {
            Name = "docs",
            RedirectUrl = targetUrl,
            CustomShortLink = shortCode
        };

        var createResponse = await _client.PostAsJsonAsync("/api/Link/create-random", request);
        Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);

        var createdLink = await createResponse.Content.ReadFromJsonAsync<ShortLink>();
        Assert.NotNull(createdLink);

        var resolveResponse = await _client.GetAsync($"/api/Link/{createdLink!.ShortUrl}");

        Assert.Equal(HttpStatusCode.Redirect, resolveResponse.StatusCode);
        Assert.Equal(targetUrl, resolveResponse.Headers.Location?.ToString());
    }

    [Fact]
    public async Task CreateCustomShortLink_WithoutAuthentication_ReturnsUnauthorized()
    {
        ClearAuthentication();

        var request = new IncomingShortLinkRequestDto
        {
            Name = "secure-link",
            RedirectUrl = "https://example.org/secure",
            CustomShortLink = $"secure-{Guid.NewGuid().ToString("N")[..6]}"
        };

        var response = await _client.PostAsJsonAsync("/api/Link/create", request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateRandomShortLink_WithAuthentication_ReturnsCreatedLink()
    {
        await AuthenticateAsync();

        var request = new IncomingShortLinkRequestDto
        {
            Name = "auth-random",
            RedirectUrl = "https://example.org/auth-random"
        };

        var response = await _client.PostAsJsonAsync("/api/Link/create", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<ShortLink>();
        Assert.NotNull(created);
        Assert.False(string.IsNullOrWhiteSpace(created!.ShortUrl));
        Assert.Equal(request.RedirectUrl, created.RedirectUrl);
    }

    [Fact]
    public async Task CreateShortLink_WithAuthentication_EmptyRedirectUrl_ReturnsBadRequest()
    {
        await AuthenticateAsync();

        var request = new IncomingShortLinkRequestDto
        {
            Name = "empty-redirect-auth",
            RedirectUrl = string.Empty
        };

        var response = await _client.PostAsJsonAsync("/api/Link/create", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateShortLink_WithAuthentication_InvalidRedirectUrl_ReturnsBadRequest()
    {
        await AuthenticateAsync();

        var request = new IncomingShortLinkRequestDto
        {
            Name = "invalid-url-auth",
            RedirectUrl = "not-a-url"
        };

        var response = await _client.PostAsJsonAsync("/api/Link/create", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("has spaces")]
    [InlineData("../../etc/passwd")]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
    public async Task CreateShortLink_WithInvalidSlugFormat_ReturnsBadRequest(string invalidSlug)
    {
        await AuthenticateAsync();

        var request = new IncomingShortLinkRequestDto
        {
            Name = "invalid-slug",
            RedirectUrl = "https://example.org/invalid-slug",
            CustomShortLink = invalidSlug
        };

        var response = await _client.PostAsJsonAsync("/api/Link/create", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DuplicateCustomShortLink_OnCreateRandom_ReturnsBadRequestOrConflict()
    {
        var slug = $"dup-rand-{Guid.NewGuid().ToString("N")[..6]}";
        var request = new IncomingShortLinkRequestDto
        {
            Name = "dup-random",
            RedirectUrl = "https://example.org/dup-random",
            CustomShortLink = slug
        };

        var first = await _client.PostAsJsonAsync("/api/Link/create-random", request);
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);

        var second = await _client.PostAsJsonAsync("/api/Link/create-random", request);
        Assert.True(second.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task DuplicateCustomShortLink_OnAuthenticatedCreate_ReturnsConflict()
    {
        await AuthenticateAsync();

        var slug = $"dup-auth-{Guid.NewGuid().ToString("N")[..6]}";
        var request = new IncomingShortLinkRequestDto
        {
            Name = "dup-auth",
            RedirectUrl = "https://example.org/dup-auth",
            CustomShortLink = slug
        };

        var first = await _client.PostAsJsonAsync("/api/Link/create", request);
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);

        var second = await _client.PostAsJsonAsync("/api/Link/create", request);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task UserLinks_WithoutAuthentication_ReturnsUnauthorized()
    {
        ClearAuthentication();

        var response = await _client.GetAsync("/api/Link/user-links");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteShortLink_WithoutAuthentication_ReturnsUnauthorized()
    {
        ClearAuthentication();

        var response = await _client.DeleteAsync($"/api/Link/delete/unauth-{Guid.NewGuid().ToString("N")[..6]}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AuthorizedUser_CanCreateListAndDeleteShortLinks()
    {
        await AuthenticateAsync();

        var customShortCode = $"auth-{Guid.NewGuid().ToString("N")[..8]}";
        var createRequest = new IncomingShortLinkRequestDto
        {
            Name = "authorized-custom",
            RedirectUrl = "https://example.org/authorized-custom",
            CustomShortLink = customShortCode
        };

        var createResponse = await _client.PostAsJsonAsync("/api/Link/create", createRequest);
        Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<ShortLink>();
        Assert.NotNull(created);
        Assert.Equal(customShortCode, created!.ShortUrl);

        var listResponse = await _client.GetAsync("/api/Link/user-links");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        var links = await listResponse.Content.ReadFromJsonAsync<List<ShortLink>>();
        Assert.NotNull(links);
        Assert.Contains(links!, l => l.ShortUrl == customShortCode);

        var deleteResponse = await _client.DeleteAsync($"/api/Link/delete/{customShortCode}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var resolveResponse = await _client.GetAsync($"/api/Link/{customShortCode}");
        Assert.Equal(HttpStatusCode.NotFound, resolveResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteAnotherUsersLink_ReturnsForbidden()
    {
        await AuthenticateAsync("owner");

        var slug = $"owner-{Guid.NewGuid().ToString("N")[..6]}";
        var createRequest = new IncomingShortLinkRequestDto
        {
            Name = "owner-link",
            RedirectUrl = "https://example.org/owner",
            CustomShortLink = slug
        };

        var createResponse = await _client.PostAsJsonAsync("/api/Link/create", createRequest);
        Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);

        await AuthenticateAsync("other");

        var deleteResponse = await _client.DeleteAsync($"/api/Link/delete/{slug}");
        Assert.Equal(HttpStatusCode.Forbidden, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task RefreshToken_Rotates_And_OldToken_NoLongerWorks()
    {
        var (_, initialTokens) = await RegisterAndLoginAsync("refresh");

        var refreshRequest = new RefreshRequest { RefreshToken = initialTokens.RefreshToken };
        var firstRefreshResponse = await _client.PostAsJsonAsync("/Auth/refresh", refreshRequest);
        Assert.Equal(HttpStatusCode.OK, firstRefreshResponse.StatusCode);

        var refreshedTokens = await firstRefreshResponse.Content.ReadFromJsonAsync<TokenResponse>();
        Assert.NotNull(refreshedTokens);
        Assert.False(string.IsNullOrWhiteSpace(refreshedTokens!.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(refreshedTokens.RefreshToken));
        Assert.NotEqual(initialTokens.RefreshToken, refreshedTokens.RefreshToken);

        var secondRefreshResponse = await _client.PostAsJsonAsync("/Auth/refresh", refreshRequest);
        Assert.True(secondRefreshResponse.StatusCode is HttpStatusCode.OK or HttpStatusCode.NoContent);

        var secondRefreshBody = await secondRefreshResponse.Content.ReadAsStringAsync();
        Assert.True(string.IsNullOrWhiteSpace(secondRefreshBody) || secondRefreshBody.Trim() == "null");
    }

    [Fact]
    public async Task Logout_InvalidatesRefreshToken()
    {
        var (_, loginTokens) = await RegisterAndLoginAsync("logout");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginTokens.AccessToken);

        var logoutResponse = await _client.PostAsync("/Auth/logout", null);
        Assert.Equal(HttpStatusCode.OK, logoutResponse.StatusCode);

        ClearAuthentication();

        var refreshResponse = await _client.PostAsJsonAsync("/Auth/refresh", new RefreshRequest
        {
            RefreshToken = loginTokens.RefreshToken
        });
        Assert.True(refreshResponse.StatusCode is HttpStatusCode.OK or HttpStatusCode.NoContent);

        var refreshBody = await refreshResponse.Content.ReadAsStringAsync();
        Assert.True(string.IsNullOrWhiteSpace(refreshBody) || refreshBody.Trim() == "null");
    }

    private async Task AuthenticateAsync(string userPrefix = "auth-user")
    {
        var (_, tokens) = await RegisterAndLoginAsync(userPrefix);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
    }

    private async Task<(RegisterModel Register, TokenResponse Tokens)> RegisterAndLoginAsync(string userPrefix)
    {
        ClearAuthentication();

        var unique = Guid.NewGuid().ToString("N")[..8];
        var register = new RegisterModel
        {
            Username = $"{userPrefix}-{unique}",
            Full_Name = "Authorized Integration User",
            Email = $"{userPrefix}-{unique}@test.local",
            Password = "TestPassword123!"
        };

        var registerResponse = await _client.PostAsJsonAsync("/Auth/Register", register);
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        var login = new LoginModel
        {
            User = register.Username,
            Password = register.Password
        };

        var loginResponse = await _client.PostAsJsonAsync("/Auth/Login", login);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var tokens = await loginResponse.Content.ReadFromJsonAsync<TokenResponse>();
        Assert.False(string.IsNullOrWhiteSpace(tokens?.AccessToken));

        return (register, tokens!);
    }

    private void ClearAuthentication()
    {
        _client.DefaultRequestHeaders.Authorization = null;
    }
}