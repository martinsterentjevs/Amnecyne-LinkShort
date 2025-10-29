using API_Auth_Demo.Models;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using API_Auth_Demo.Data;

namespace API_Auth_Demo.Services
{
    public class DBStorageService
    {
        private readonly AppDbContext _db;
        public DBStorageService(AppDbContext db)
        {
            _db = db;
        }

        internal object GetUserByIdentifier(string userid)
        {
            var filteredUsers = _db.Users
                .Where(u => u.Username == userid || u.Email == userid)
                .ToList();
            var user = filteredUsers.FirstOrDefault();
            return user;
        }
        internal void AddUser(User user)
        {
            _db.Users.Add(user);
            _db.SaveChanges();
        }
        internal void RemoveUser(User user)
        {
            _db.Users.Remove(user);
            _db.SaveChanges();
        }
        public void UpdateUser(User user)
        {
            _db.Users.Update(user);
            _db.SaveChanges();
        }
        internal bool IsUsernameTaken(string username)
        {
            return _db.Users.Any(u => u.Username == username);
        }
        internal bool IsEmailTaken(string email)
        {
            return _db.Users.Any(u => u.Email == email);
        }
        internal bool IsPasswordHashValid(User user, string passwordHash)
        {
            if (user == null)
            {
                return false;
            }
            return user.PasswordHash == passwordHash;
        }
        public void SaveRefreshToken(User user, RefreshTokens refreshToken)
        {
            _db.RefreshTokens.Add(refreshToken);
            user.refreshToken = refreshToken;
            _db.SaveChanges();
        }
        public User GetUserByRefToken(string token)
        {
            var user = _db.Users.FirstOrDefault(u => u.refreshToken != null && u.refreshToken.Token == token);
            return user;
        }
        public void MarkRefTokenInvalid(RefreshTokens refreshToken)
        {
            refreshToken.IsRevoked = true;
            _db.RefreshTokens.Update(refreshToken);
            _db.SaveChanges();
        }
    }
}
