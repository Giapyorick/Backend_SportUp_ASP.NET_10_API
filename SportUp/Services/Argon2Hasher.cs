using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;

namespace SportUp.Services
{
    public static class Argon2Hasher
    {
        // 1. Hash password bằng Argon2id
        public static string HashPassword(string password)
        {
            // Sinh ngẫu nhiên 16 bytes Salt
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
            {
                Salt = salt,
                DegreeOfParallelism = 8, // Số luồng CPU
                MemorySize = 65536,     // 64 MB RAM
                Iterations = 4          // Số vòng lặp
            };

            byte[] hash = argon2.GetBytes(32); // Hash dài 32 bytes

            // Trả về chuỗi định dạng: {Salt_Base64}:{Hash_Base64}
            return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
        }

        // 2. Kiểm tra password nhập vào có khớp với Argon2 Hash hay không
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            var parts = hashedPassword.Split(':');
            if (parts.Length != 2) return false;

            byte[] salt = Convert.FromBase64String(parts[0]);
            byte[] expectedHash = Convert.FromBase64String(parts[1]);

            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
            {
                Salt = salt,
                DegreeOfParallelism = 8,
                MemorySize = 65536,
                Iterations = 4
            };

            byte[] actualHash = argon2.GetBytes(32);

            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
    }
}