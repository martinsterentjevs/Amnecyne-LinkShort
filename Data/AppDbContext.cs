using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using API_Auth_Demo.Models;
namespace API_Auth_Demo.Data
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
