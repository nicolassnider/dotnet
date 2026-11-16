using System.Security.Cryptography;

namespace DotNetMastery.Core.Security;

/// <summary>
/// Implements secure password hashing adhering to OWASP recommendations:
/// - Uses PBKDF2 (Rfc2898DeriveBytes) with HMACSHA512
/// - 100,000+ iterations
/// - 128-bit (16 bytes) cryptographically random salt (CSPRNG)
/// - Constant-time comparison (FixedTimeEquals) to prevent timing side-channel attacks
/// </summary>
public static class PasswordHasher
{
    private const int SaltSize = 16;       // 128 bits
    private const int KeySize = 32;        // 256 bits
    private const int Iterations = 100_000;
    private static readonly HashAlgorithmName HashAlgorithm = HashAlgorithmName.SHA512;

    public static string HashPassword(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithm,
            KeySize);

        // Format: {iterations}.{salt-base64}.{hash-base64}
        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public static bool VerifyPassword(string password, string hashedPassword)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        ArgumentException.ThrowIfNullOrWhiteSpace(hashedPassword);

        string[] parts = hashedPassword.Split('.', 3);
        if (parts.Length != 3)
        {
            return false;
        }

        if (!int.TryParse(parts[0], out int iterations))
        {
            return false;
        }

        byte[] salt = Convert.FromBase64String(parts[1]);
        byte[] expectedHash = Convert.FromBase64String(parts[2]);

        byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            iterations,
            HashAlgorithm,
            expectedHash.Length);

        // Fixed-time comparison protects against timing analysis attacks
        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }
}
