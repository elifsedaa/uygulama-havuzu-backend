using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UygulamaHavuzu.Application.Interfaces;

namespace UygulamaHavuzu.Application.Services
{
    public class PasswordService : IPasswordService
    {
        // Salt boyutu: 256 bit (güvenlik için rastgele veri)
        private const int SaltSize = 32;
        // Hash boyutu: 256 bit
        private const int HashSize = 32;
        // PBKDF2 algoritmasý için iterasyon sayýsý (hesaplama maliyeti/güvenlik dengesi)
        private const int Iterations = 10000;

        public string HashPassword(string password)
        {
            try
            {
                if (string.IsNullOrEmpty(password))
                    throw new ArgumentException("Þifre boþ olamaz", nameof(password));

                byte[] salt = new byte[SaltSize];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(salt); // Rastgele salt oluþturulur
                }

                using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
                {
                    byte[] hash = pbkdf2.GetBytes(HashSize); // PBKDF2 ile þifre hashlenir

                    byte[] hashBytes = new byte[SaltSize + HashSize];
                    Array.Copy(salt, 0, hashBytes, 0, SaltSize); // Ýlk kýsma salt kopyalanýr
                    Array.Copy(hash, 0, hashBytes, SaltSize, HashSize); // Sonrasýna hash kopyalanýr

                    return Convert.ToBase64String(hashBytes); // Salt + hash base64 ile dönülür
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Þifre hash'lenirken hata oluþtu", ex);
            }
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            try
            {
                if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hashedPassword))
                    return false;

                byte[] hashBytes = Convert.FromBase64String(hashedPassword); // Base64 çözülür

                if (hashBytes.Length != SaltSize + HashSize)
                    return false; // Geçersiz formatta ise false döner

                byte[] salt = new byte[SaltSize];
                Array.Copy(hashBytes, 0, salt, 0, SaltSize); // Salt ayýklanýr

                byte[] storedHash = new byte[HashSize];
                Array.Copy(hashBytes, SaltSize, storedHash, 0, HashSize); // Orijinal hash alýnýr

                using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
                {
                    byte[] computedHash = pbkdf2.GetBytes(HashSize); // Girilen þifre hashlenir

                    return SlowEquals(storedHash, computedHash); // Karþýlaþtýrýlýr
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

            // Þifrenin karmaþýklýðýný garanti altýna almak için her karakter türünden en az bir tane eklenir
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

            return new string(result.ToString().ToCharArray().OrderBy(x => random.Next()).ToArray()); // Karakterler karýþtýrýlýr
        }

        public (bool IsValid, List<string> Errors) ValidatePassword(string password)
        {
            var errors = new List<string>();

            if (string.IsNullOrEmpty(password))
            {
                errors.Add("Þifre boþ olamaz");
                return (false, errors);
            }

            if (password.Length < 8)
                errors.Add("Þifre en az 8 karakter olmalýdýr");

            if (password.Length > 128)
                errors.Add("Þifre 128 karakterden fazla olamaz");

            if (!Regex.IsMatch(password, @"[a-z]"))
                errors.Add("Þifre en az bir küçük harf içermelidir");

            if (!Regex.IsMatch(password, @"[A-Z]"))
                errors.Add("Þifre en az bir büyük harf içermelidir");

            if (!Regex.IsMatch(password, @"\d"))
                errors.Add("Þifre en az bir rakam içermelidir");

            if (!Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{}|;:,.<>?]"))
                errors.Add("Þifre en az bir özel karakter içermelidir (!@#$%^&* vb.)");

            var commonPasswords = new[]
            {
                "password", "123456", "123456789", "qwerty", "abc123",
                "password123", "admin", "letmein", "welcome", "monkey"
            };

            if (commonPasswords.Contains(password.ToLower()))
                errors.Add("Çok yaygýn kullanýlan bir þifre seçtiniz. Lütfen daha güvenli bir þifre seçin");

            if (HasSequentialChars(password))
                errors.Add("Þifre ardýþýk karakterler içermemelidir (123456, abcdef vb.)");

            if (HasRepeatingChars(password))
                errors.Add("Þifre çok fazla tekrarlanan karakter içermemelidir");

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

            return diff == 0; // Tüm baytlar karþýlaþtýrýlýr, zamanlama saldýrýlarýna karþý korunur
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
