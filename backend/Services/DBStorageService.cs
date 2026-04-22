using Amnecyne.LinkShort.Data;
using Amnecyne.LinkShort.Models;
using Microsoft.EntityFrameworkCore;

namespace Amnecyne.LinkShort.Services;

public class DBStorageService
{
    private readonly AppDbContext _db;

    public DBStorageService(AppDbContext db)
    {
        _db = db;
    }

    internal async Task<User> GetUserByIdentifierAsync(string userid)
    {
        var user = await _db.Users
                       .Include(u => u.refreshToken)
                       .FirstOrDefaultAsync(u => u.Username == userid || u.Email == userid)
                   ?? throw new Exception("User not found.");
        return user;
    }

    internal async Task AddUserAsync(User user)
    {
        await _db.Users.AddAsync(user);
        await _db.SaveChangesAsync();
    }

    internal async Task RemoveUserAsync(User user)
    {
        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateUserAsync(User user)
    {
        _db.Users.Update(user);
        await _db.SaveChangesAsync();
    }

    internal async Task<bool> IsUsernameTakenAsync(string username)
    {
        return await _db.Users.AnyAsync(u => u.Username == username);
    }

    internal async Task<bool> IsEmailTakenAsync(string email)
    {
        return await _db.Users.AnyAsync(u => u.Email == email);
    }

    internal bool IsPasswordHashValid(User user, string passwordHash)
    {
        if (user == null) return false;
        return user.PasswordHash == passwordHash;
    }

    public async Task SaveRefreshTokenAsync(User user, RefreshTokens refreshToken)
    {
        await _db.RefreshTokens.AddAsync(refreshToken);
        user.refreshToken = refreshToken;
        await _db.SaveChangesAsync();
    }

    public async Task<User?> GetUserByRefTokenAsync(string token)
    {
        var user = await _db.Users
            .Include(u => u.refreshToken)
            .FirstOrDefaultAsync(u => u.refreshToken != null && u.refreshToken.Token == token);
        return user;
    }

    public async Task MarkRefTokenInvalidAsync(RefreshTokens refreshToken)
    {
        refreshToken.IsRevoked = true;
        _db.RefreshTokens.Update(refreshToken);
        await _db.SaveChangesAsync();
    }

    // ShortLink related methods
    public async Task AddShortLinkAsync(ShortLink shortLink)
    {
        await _db.ShortLinks.AddAsync(shortLink);
        await _db.SaveChangesAsync();
    }

    public async Task<ShortLink> GetShortLinkByNameAsync(string name)
    {
        return await _db.ShortLinks.FirstOrDefaultAsync(sl => sl.Name == name) ??
               throw new Exception("Short link not found.");
    }

    public async Task<ShortLink> GetShortLinkByIdAsync(string link)
    {
        try
        {
            if (string.IsNullOrEmpty(link)) throw new ArgumentException("Link cannot be null or empty.", nameof(link));
            return await _db.ShortLinks.FirstOrDefaultAsync(sl => sl.ShortUrl == link) ??
                   throw new Exception("Short link not found.");
        }
        catch (Exception)
        {
            throw new Exception("Error retrieving short link.");
        }
    }

    public async Task RemoveShortLinkAsync(ShortLink shortLink)
    {
        _db.ShortLinks.Remove(shortLink);
        await _db.SaveChangesAsync();
    }

    public async Task<List<ShortLink>> GetAllShortLinksAsync(string userId)
    {
        return await _db.ShortLinks.Where(s => s.UserId == userId).ToListAsync();
    }

    internal async Task<bool> IsShortUrlTakenAsync(string customShortLink)
    {
        return await _db.ShortLinks.AnyAsync(sl => sl.ShortUrl == customShortLink);
    }
}