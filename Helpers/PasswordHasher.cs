using BCrypt.Net;

namespace StockifyPlus.Helpers
{
    public static class PasswordHasher
    {
        private const int WorkFactor = 12;

        public static string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Şifre boş olamaz.", nameof(password));

            return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
        }

        public static bool VerifyPassword(string password, string hash)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            if (string.IsNullOrWhiteSpace(hash))
                return false;

            if (hash.StartsWith("$2"))
            {
                try
                {
                    return BCrypt.Net.BCrypt.Verify(password, hash);
                }
                catch
                {
                    return false;
                }
            }

            if (hash.Length == 44 && !hash.Contains('$'))
            {
                var legacyHash = ComputeLegacySHA256Hash(password);
                return hash == legacyHash;
            }

            return false;
        }

        public static bool IsBcryptHash(string hash)
        {
            if (string.IsNullOrWhiteSpace(hash))
                return false;

            return hash.StartsWith("$2");
        }

        private static string ComputeLegacySHA256Hash(string password)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        public static bool IsPasswordStrong(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            if (password.Length < 8)
                return false;

            if (!password.Any(char.IsUpper))
                return false;

            if (!password.Any(char.IsLower))
                return false;

            if (!password.Any(char.IsDigit))
                return false;

            var specialChars = "!@#$%^&*()_+-=[]{}|;:,.<>?";
            if (!password.Any(c => specialChars.Contains(c)))
                return false;

            return true;
        }

        public static List<string> GetPasswordValidationErrors(string password)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(password))
            {
                errors.Add("Şifre boş olamaz.");
                return errors;
            }

            if (password.Length < 8)
                errors.Add("Şifre en az 8 karakter olmalıdır.");

            if (!password.Any(char.IsUpper))
                errors.Add("Şifre en az bir büyük harf içermelidir.");

            if (!password.Any(char.IsLower))
                errors.Add("Şifre en az bir küçük harf içermelidir.");

            if (!password.Any(char.IsDigit))
                errors.Add("Şifre en az bir rakam içermelidir.");

            var specialChars = "!@#$%^&*()_+-=[]{}|;:,.<>?";
            if (!password.Any(c => specialChars.Contains(c)))
                errors.Add("Şifre en az bir özel karakter içermelidir (!@#$%^&* vb.)");

            return errors;
        }
    }
}
