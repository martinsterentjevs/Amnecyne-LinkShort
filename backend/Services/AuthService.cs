using Amnecyne.LinkShort.DTOs;
using Amnecyne.LinkShort.Helpers;
using Amnecyne.LinkShort.Models;
using Amnecyne.LinkShort.Models.enums;

namespace Amnecyne.LinkShort.Services;

public class AuthService
{
    private readonly DBStorageService _dbStorageService;
    private readonly TokenService _tokenService;

    public AuthService(DBStorageService dbStorageService, TokenService tokenService)
    {
        _dbStorageService = dbStorageService;
        _tokenService = tokenService;
    }

    public async Task<string> ValidateUserAsync(string username, string password)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            return ErrorMessages.InvalidCredentials.ToString();
        var user = await _dbStorageService.GetUserByIdentifierAsync(username);
        if (user == null) return ErrorMessages.UserNotFound.ToString();
        var isValid = PBKDF2.VerifyPassword(password, user.Salt, user.PasswordHash);
        return isValid ? "Success" : ErrorMessages.UnknownError.ToString();
    }

    public async Task<TokenResponse?> RegisterUserAsync(string username, string fullname, string email, string password)
    {
        try
        {
            if (await IsUserDataTakenAsync(username, email)) return null;
            var salt = PBKDF2.GenerateSalt();
            var passwordHash = PBKDF2.HashPassword(password, salt);
            var newUser = new User
            {
                Username = username,
                Full_Name = fullname,
                Email = email,
                PasswordHash = passwordHash,
                Salt = salt
            };
            await _dbStorageService.AddUserAsync(newUser);
            var tokens = _tokenService.GenerateTokens(newUser);
            await _dbStorageService.SaveRefreshTokenAsync(newUser, new RefreshTokens
            {
                UserId = newUser.Id.ToString(),
                Token = tokens.RefreshToken,
                Expires = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            });
            return tokens;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private async Task<bool> IsUserDataTakenAsync(string username, string email)
    {
        if (await _dbStorageService.IsUsernameTakenAsync(username)) return true;
        if (await _dbStorageService.IsEmailTakenAsync(email)) return true;
        return false;
    }

    public async Task<TokenResponse?> LoginUserAsync(string username, string pw)
    {
        var validationResponse = await ValidateUserAsync(username, pw);
        if (validationResponse == "Success")
        {
            var user = await _dbStorageService.GetUserByIdentifierAsync(username);
            var tokens = _tokenService.GenerateTokens(user);
            await _dbStorageService.SaveRefreshTokenAsync(user, new RefreshTokens
            {
                UserId = user.Id.ToString(),
                Token = tokens.RefreshToken,
                Expires = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            });

            return tokens;
        }

        return null;
    }

    public async Task<TokenResponse?> RefreshTokensAsync(string refreshToken)
    {
        var user = await _dbStorageService.GetUserByRefTokenAsync(refreshToken);
        if (user == null || user.refreshToken == null)
            return null;

        if (user.refreshToken.Expires < DateTime.UtcNow || user.refreshToken.IsRevoked)
            return null;
        var oldRef = user.refreshToken;
        await _dbStorageService.MarkRefTokenInvalidAsync(oldRef);
        var newTokens = _tokenService.GenerateTokens(user);
        var newRef = new RefreshTokens
        {
            UserId = user.Id.ToString(),
            Token = newTokens.RefreshToken,
            Expires = DateTime.UtcNow.AddDays(7),
            IsRevoked = false
        };
        await _dbStorageService.SaveRefreshTokenAsync(user, newRef);
        return newTokens;
    }

    public async Task LogoutUserAsync(string user)
    {
        var userObj = await _dbStorageService.GetUserByIdentifierAsync(user);
        if (userObj != null)
        {
            var refToken = userObj.refreshToken;
            if (refToken != null) await _dbStorageService.MarkRefTokenInvalidAsync(refToken);
            userObj.refreshToken = null;
            await _dbStorageService.UpdateUserAsync(userObj);
        }
    }
}