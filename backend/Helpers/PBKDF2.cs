using System.Security.Cryptography;
using System.Text;

namespace Amnecyne.LinkShort.Helpers;

public class PBKDF2
{
    private const int DefaultIterations = 100_000;
    private const int DefaultSaltByteSize = 16;
    private const int DefaultHashByteSize = 32;

    public static string GenerateSalt(int saltByteSize = DefaultSaltByteSize)
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(saltByteSize));
    }

    public static string HashPassword(string password, string salt, int iterations = DefaultIterations,
        int hashByteSize = DefaultHashByteSize)
    {
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        var saltBytes = Convert.FromBase64String(salt);
        var hash = new byte[hashByteSize];

        Rfc2898DeriveBytes.Pbkdf2(
            passwordBytes,
            saltBytes,
            hash,
            iterations,
            HashAlgorithmName.SHA256);

        return Convert.ToBase64String(hash);
    }

    public static bool VerifyPassword(string password, string salt, string expectedHash,
        int iterations = DefaultIterations, int hashByteSize = DefaultHashByteSize)
    {
        var computedHash = HashPassword(password, salt, iterations, hashByteSize);
        var computedHashBytes = Convert.FromBase64String(computedHash);
        var expectedHashBytes = Convert.FromBase64String(expectedHash);

        return CryptographicOperations.FixedTimeEquals(computedHashBytes, expectedHashBytes);
    }
}