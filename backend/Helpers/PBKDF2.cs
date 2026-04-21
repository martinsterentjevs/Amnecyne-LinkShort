namespace Amnecyne.LinkShort.Helpers
{
    public class PBKDF2
    {
        public static string HashPassword(string password, string salt, int iterations = 10000, int hashByteSize = 32)
        {
            using (var rfc2898 = new System.Security.Cryptography.Rfc2898DeriveBytes(password, System.Text.Encoding.UTF8.GetBytes(salt), iterations, System.Security.Cryptography.HashAlgorithmName.SHA256))
            {
                var hash = rfc2898.GetBytes(hashByteSize);
                return Convert.ToBase64String(hash);
            }
        }
    }
}
