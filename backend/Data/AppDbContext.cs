using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Amnecyne.LinkShort.Models;
namespace Amnecyne.LinkShort.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<RefreshTokens> RefreshTokens { get; set; }
    }
}
