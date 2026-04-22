using System.Security.Claims;
using Amnecyne.LinkShort.DTOs;
using Amnecyne.LinkShort.Models.enums;
using Amnecyne.LinkShort.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Amnecyne.LinkShort.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : Controller
{
    private readonly AuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(ILogger<AuthController> logger, AuthService authService)
    {
        _logger = logger;
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("Login")]
    public async Task<IActionResult> Login(LoginModel dto)
    {
        var loginResult = await _authService.LoginUserAsync(dto.User, dto.Password);
        if (loginResult == null) return Unauthorized(new { message = "Invalid username or password." });

        return Ok(loginResult);
    }

    [AllowAnonymous]
    [HttpPost("Register")]
    public async Task<IActionResult> Register(RegisterModel dto)
    {
        if (dto.Full_Name == null || dto.Username == null || dto.Email == null || dto.Password == null)
            return BadRequest(new { message = "Error. Make sure all fields are filled in." });
        if (dto.Password.Length < 8)
            return BadRequest(new { message = "Password must be at least 8 characters long." });
        var registerResult = await _authService.RegisterUserAsync(dto.Username, dto.Full_Name, dto.Email, dto.Password);
        if (registerResult == null) return BadRequest(new { message = ErrorMessages.UsernameAlreadyExists.ToString() });
        return Ok(registerResult);
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> TokenRefresh(RefreshRequest refreshRequest)
    {
        var newTokens = await _authService.RefreshTokensAsync(refreshRequest.RefreshToken);
        return Ok(newTokens);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var username = User.FindFirstValue(ClaimTypes.Name);
        if (userId == null || username == null) return Unauthorized(new { message = "User not authenticated." });
        await _authService.LogoutUserAsync(username);
        return Ok(new { message = "Successfully logged out." });
    }
}