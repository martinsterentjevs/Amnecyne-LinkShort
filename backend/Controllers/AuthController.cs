using Amnecyne.LinkShort.DTOs;
using Amnecyne.LinkShort.Models.enums;
using Amnecyne.LinkShort.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Amnecyne.LinkShort.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : Controller
    {
        private readonly ILogger<AuthController> _logger;
        private readonly AuthService _authService;
        public AuthController(ILogger<AuthController> logger, AuthService authService)
        {
            _logger = logger;
            _authService = authService;
        }
        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login(LoginModel dto)
        {
            var loginResult = _authService.LoginUser(dto.User, dto.Password);
            if (loginResult == null)
            {
                return Unauthorized(new { message = "Invalid username or password." });
            }

            return Ok(loginResult);
        }

        [AllowAnonymous]
        [HttpPost("Register")]
        public IActionResult Register(RegisterModel dto)
        {
            if (dto.Full_Name == null || dto.Username == null || dto.Email == null || dto.Password == null)
            {
                return BadRequest(new { message = "Error. Make sure all fields are filled in." });
            }
            if (dto.Password.Length < 8)
            {
                return BadRequest(new { message = "Password must be at least 8 characters long." });
            }
            var registerResult = _authService.RegisterUser(dto.Username, dto.Full_Name, dto.Email, dto.Password);
            if (registerResult == null)
            {
                return BadRequest(new { message = ErrorMessages.UsernameAlreadyExists.ToString() });
            }
            return Ok(registerResult);
        }
        [AllowAnonymous]
        [HttpPost("refresh")]
        public IActionResult TokenRefresh(DTOs.RefreshRequest refreshRequest)
        {
            var newTokens = _authService.RefreshTokens(refreshRequest.RefreshToken);
            return Ok(newTokens);
        }
        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var username = User.FindFirstValue(ClaimTypes.Name);
            _authService.LogoutUser(username);
            return Ok(new { message = "Successfully logged out." });
        }
    }
}
