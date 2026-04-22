using Amnecyne.LinkShort.Models;
using Microsoft.EntityFrameworkCore;

namespace Amnecyne.LinkShort.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<RefreshTokens> RefreshTokens { get; set; }
    public DbSet<ShortLink> ShortLinks { get; set; }
}