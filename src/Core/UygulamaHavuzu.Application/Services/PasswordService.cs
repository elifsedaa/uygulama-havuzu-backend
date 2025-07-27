using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UygulamaHavuzu.Application.Interfaces;

namespace UygulamaHavuzu.Application.Services
{
    public class PasswordService : IPasswordService
    {
        // Salt boyutu: 256 bit (g�venlik i�in rastgele veri)
        private const int SaltSize = 32;
        // Hash boyutu: 256 bit
        private const int HashSize = 32;
        // PBKDF2 algoritmas� i�in iterasyon say�s� (hesaplama maliyeti/g�venlik dengesi)
        private const int Iterations = 10000;

        public string HashPassword(string password)
        {
            try
            {
                if (string.IsNullOrEmpty(password))
                    throw new ArgumentException("�ifre bo� olamaz", nameof(password));

                byte[] salt = new byte[SaltSize];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(salt); // Rastgele salt olu�turulur
                }

                using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
                {
                    byte[] hash = pbkdf2.GetBytes(HashSize); // PBKDF2 ile �ifre hashlenir

                    byte[] hashBytes = new byte[SaltSize + HashSize];
                    Array.Copy(salt, 0, hashBytes, 0, SaltSize); // �lk k�sma salt kopyalan�r
                    Array.Copy(hash, 0, hashBytes, SaltSize, HashSize); // Sonras�na hash kopyalan�r

                    return Convert.ToBase64String(hashBytes); // Salt + hash base64 ile d�n�l�r
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("�ifre hash'lenirken hata olu�tu", ex);
            }
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            try
            {
                if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hashedPassword))
                    return false;

                byte[] hashBytes = Convert.FromBase64String(hashedPassword); // Base64 ��z�l�r

                if (hashBytes.Length != SaltSize + HashSize)
                    return false; // Ge�ersiz formatta ise false d�ner

                byte[] salt = new byte[SaltSize];
                Array.Copy(hashBytes, 0, salt, 0, SaltSize); // Salt ay�klan�r

                byte[] storedHash = new byte[HashSize];
                Array.Copy(hashBytes, SaltSize, storedHash, 0, HashSize); // Orijinal hash al�n�r

                using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
                {
                    byte[] computedHash = pbkdf2.GetBytes(HashSize); // Girilen �ifre hashlenir

                    return SlowEquals(storedHash, computedHash); // Kar��la�t�r�l�r
                }
            }
            catch
            {
                return false;
            }
        }

        public string GenerateRandomPassword(int length = 12, bool includeSpecialChars = true)
        {
            if (length < 6)
                length = 6;

            const string lowercase = "abcdefghijklmnopqrstuvwxyz";
            const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string digits = "0123456789";
            const string specialChars = "!@#$%^&*()_+-=[]{}|;:,.<>?";

            string chars = lowercase + uppercase + digits;
            if (includeSpecialChars)
                chars += specialChars;

            var random = new Random();
            var result = new StringBuilder();

            // �ifrenin karma��kl���n� garanti alt�na almak i�in her karakter t�r�nden en az bir tane eklenir
            result.Append(lowercase[random.Next(lowercase.Length)]);
            result.Append(uppercase[random.Next(uppercase.Length)]);
            result.Append(digits[random.Next(digits.Length)]);
            if (includeSpecialChars)
                result.Append(specialChars[random.Next(specialChars.Length)]);

            int remainingLength = length - result.Length;
            for (int i = 0; i < remainingLength; i++)
            {
                result.Append(chars[random.Next(chars.Length)]);
            }

            return new string(result.ToString().ToCharArray().OrderBy(x => random.Next()).ToArray()); // Karakterler kar��t�r�l�r
        }

        public (bool IsValid, List<string> Errors) ValidatePassword(string password)
        {
            var errors = new List<string>();

            if (string.IsNullOrEmpty(password))
            {
                errors.Add("�ifre bo� olamaz");
                return (false, errors);
            }

            if (password.Length < 8)
                errors.Add("�ifre en az 8 karakter olmal�d�r");

            if (password.Length > 128)
                errors.Add("�ifre 128 karakterden fazla olamaz");

            if (!Regex.IsMatch(password, @"[a-z]"))
                errors.Add("�ifre en az bir k���k harf i�ermelidir");

            if (!Regex.IsMatch(password, @"[A-Z]"))
                errors.Add("�ifre en az bir b�y�k harf i�ermelidir");

            if (!Regex.IsMatch(password, @"\d"))
                errors.Add("�ifre en az bir rakam i�ermelidir");

            if (!Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{}|;:,.<>?]"))
                errors.Add("�ifre en az bir �zel karakter i�ermelidir (!@#$%^&* vb.)");

            var commonPasswords = new[]
            {
                "password", "123456", "123456789", "qwerty", "abc123",
                "password123", "admin", "letmein", "welcome", "monkey"
            };

            if (commonPasswords.Contains(password.ToLower()))
                errors.Add("�ok yayg�n kullan�lan bir �ifre se�tiniz. L�tfen daha g�venli bir �ifre se�in");

            if (HasSequentialChars(password))
                errors.Add("�ifre ard���k karakterler i�ermemelidir (123456, abcdef vb.)");

            if (HasRepeatingChars(password))
                errors.Add("�ifre �ok fazla tekrarlanan karakter i�ermemelidir");

            return (errors.Count == 0, errors);
        }

        private static bool SlowEquals(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;

            uint diff = 0;
            for (int i = 0; i < a.Length; i++)
            {
                diff |= (uint)(a[i] ^ b[i]);
            }

            return diff == 0; // T�m baytlar kar��la�t�r�l�r, zamanlama sald�r�lar�na kar�� korunur
        }

        private static bool HasSequentialChars(string password)
        {
            for (int i = 0; i < password.Length - 2; i++)
            {
                if ((password[i] + 1 == password[i + 1]) && (password[i + 1] + 1 == password[i + 2]))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool HasRepeatingChars(string password)
        {
            for (int i = 0; i < password.Length - 3; i++)
            {
                if (password[i] == password[i + 1] &&
                    password[i + 1] == password[i + 2] &&
                    password[i + 2] == password[i + 3])
                {
                    return true;
                }
            }
            return false;
        }
    }
}
