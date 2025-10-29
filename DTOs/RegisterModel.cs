namespace API_Auth_Demo.DTOs
{
    public class RegisterModel
    {
        public required string Username { get; set; }
        public required string Full_Name { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}