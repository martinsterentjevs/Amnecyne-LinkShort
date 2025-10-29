namespace API_Auth_Demo.DTOs
{
    public class TokenResponse
    {
        public required string AccessToken { get; set; }
        public required string RefreshToken { get; set; }
    }
}
