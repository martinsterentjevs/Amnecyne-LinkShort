namespace Amnecyne.LinkShort.Models
{
    public class User
    {
        public int Id { get; set; }
        public required string Username { get; set; }
        public required string Full_Name { get; set; } 
        public required string PasswordHash { get; set; }
        public required string Email { get; set; }
        public required string Salt { get; set; }
        public RefreshTokens? refreshToken { get; set; }
    }
}
