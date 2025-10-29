using API_Auth_Demo.Models;
using API_Auth_Demo.Models.enums;
using API_Auth_Demo.Helpers;
using System.Security.Cryptography;
using API_Auth_Demo.DTOs;

namespace API_Auth_Demo.Services
{
    public class AuthService
    {
        private readonly DBStorageService _dbStorageService;
        private readonly TokenService _tokenService;
        public AuthService(DBStorageService dbStorageService, TokenService tokenService)
        {
            _dbStorageService = dbStorageService;
            _tokenService = tokenService;
        }
        public string ValidateUser(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                return ErrorMessages.InvalidCredentials.ToString();
            }
            var user = _dbStorageService.GetUserByIdentifier(username) as User;
            if (user == null)
            {
                return ErrorMessages.UserNotFound.ToString();
            }
            var incomingPW = PBKDF2.HashPassword(password, user.Salt, 10000, 32);
            bool isValid = _dbStorageService.IsPasswordHashValid(user, incomingPW);
            return isValid ? "Success" : ErrorMessages.UnknownError.ToString();
        }
        public TokenResponse RegisterUser(string username,string fullname, string email, string password)
        {
            try
            {
                if (IsUserDataTaken(username, email)) {return null; }
                var salt = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
                var passwordHash = PBKDF2.HashPassword(password, salt, 10000, 32);
                var newUser = new User
                {
                    Username = username,
                    Full_Name = fullname,
                    Email = email,
                    PasswordHash = passwordHash,
                    Salt = salt
                };
                _dbStorageService.AddUser(newUser);
                var tokens = _tokenService.GenerateTokens(newUser);
                return tokens;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private bool IsUserDataTaken(string username, string email)
        {
            if (_dbStorageService.IsUsernameTaken(username))
            {
                return true;
            }
            if (_dbStorageService.IsEmailTaken(email))
            {
                return true;
            }
            return false;
        }

        public TokenResponse LoginUser(string username, string pw)
        {
            var validationResponse = ValidateUser(username, pw);
            if (validationResponse == "Success")
            {
                var user = _dbStorageService.GetUserByIdentifier(username) as User;
                
                return _tokenService.GenerateTokens(user);
            }
            else
            {
                return null;
            }
        }

        public TokenResponse RefreshTokens(string refreshToken)
        {
            var user = _dbStorageService.GetUserByRefToken(refreshToken);
            if (user == null || user.refreshToken == null)
                return null;

            if (user.refreshToken.Expires < DateTime.UtcNow || user.refreshToken.IsRevoked)
                return null;
            var oldRef = user.refreshToken;
            _dbStorageService.MarkRefTokenInvalid(oldRef);
            var newTokens = _tokenService.GenerateTokens(user);
            var newRef = new RefreshTokens
            {
                UserId = user.Id.ToString(),
                Token = newTokens.RefreshToken,
                Expires = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };
            _dbStorageService.SaveRefreshToken(user, newRef);
            return newTokens;

        }
        public void LogoutUser(string user)
        {
            var userObj = _dbStorageService.GetUserByIdentifier(user) as User;
            if (userObj != null)
            {
                var refToken = userObj.refreshToken;
                if (refToken != null)
                {
                    _dbStorageService.MarkRefTokenInvalid(refToken);
                }
                userObj.refreshToken = null;
                _dbStorageService.UpdateUser(userObj);
            }
        }
    } 
}
