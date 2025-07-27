using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UygulamaHavuzu.Application.Interfaces;

namespace UygulamaHavuzu.Application.Services
{
    // JWT i�lemlerini y�neten servis s�n�f�
    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _configuration;
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _normalExpirationMinutes;
        private readonly int _rememberMeExpirationDays;

        // Constructor: appsettings.json dosyas�ndan JWT ayarlar�n� okur
        public JwtTokenService(IConfiguration configuration)
        {
            _configuration = configuration;

            // Secret key, issuer ve audience de�erlerini al
            _secretKey = _configuration["JWT:SecretKey"] ?? throw new ArgumentNullException("JWT:SecretKey configuration is missing");
            _issuer = _configuration["JWT:Issuer"] ?? "UygulamaHavuzu";
            _audience = _configuration["JWT:Audience"] ?? "UygulamaHavuzu-Users";

            // Token s�resi ayarlar�n� al
            _normalExpirationMinutes = int.Parse(_configuration["JWT:ExpirationMinutes"] ?? "60");
            _rememberMeExpirationDays = int.Parse(_configuration["JWT:RememberMeExpirationDays"] ?? "30");
        }

        // Kullan�c� bilgilerine g�re JWT token �retir
        public string GenerateToken(int userId, string username, string email, string role, bool rememberMe = false)
        {
            try
            {
                // G�venlik anahtar� ve imza olu�tur
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                // Token i�ine eklenecek claim'ler
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Email, email),
                    new Claim(ClaimTypes.Role, role),
                    new Claim("userId", userId.ToString()),
                    new Claim("username", username),
                    new Claim("email", email),
                    new Claim("role", role),
                    new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                // Token ge�erlilik s�resi belirlenir
                var expiration = rememberMe
                    ? DateTime.UtcNow.AddDays(_rememberMeExpirationDays)
                    : DateTime.UtcNow.AddMinutes(_normalExpirationMinutes);

                // JWT token nesnesi olu�turulur
                var token = new JwtSecurityToken(
                    issuer: _issuer,
                    audience: _audience,
                    claims: claims,
                    expires: expiration,
                    signingCredentials: credentials
                );

                // Token string olarak d�nd�r�l�r
                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Token olu�turulurken hata olu�tu", ex);
            }
        }

        // Token'� do�rular, ge�erliyse kullan�c� ID'sini d�ner
        public int? ValidateToken(string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                    return null;

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_secretKey);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _issuer,
                    ValidateAudience = true,
                    ValidAudience = _audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                // Token do�rulama i�lemi yap�l�r
                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);

                // Kullan�c� ID claim'i al�n�r
                var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier) ?? principal.FindFirst("userId");

                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    return userId;
                }

                return null;
            }
            catch (SecurityTokenExpiredException)
            {
                // Token s�resi dolmu�sa null d�ner
                return null;
            }
            catch (SecurityTokenException)
            {
                // Ge�ersiz token
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        // Token'�n s�resinin dolup dolmad���n� kontrol eder
        public bool IsTokenExpired(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jsonToken = tokenHandler.ReadJwtToken(token);

                return jsonToken.ValidTo < DateTime.UtcNow;
            }
            catch
            {
                // Hatal� token varsa s�resi dolmu� kabul edilir
                return true;
            }
        }

        // Token i�indeki t�m claim'leri dictionary olarak d�ner
        public Dictionary<string, string>? GetTokenClaims(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jsonToken = tokenHandler.ReadJwtToken(token);

                var claims = new Dictionary<string, string>();

                foreach (var claim in jsonToken.Claims)
                {
                    claims[claim.Type] = claim.Value;
                }

                return claims;
            }
            catch
            {
                return null;
            }
        }

        // Token'�n ge�erlilik biti� zaman�n� d�ner
        public DateTime? GetTokenExpiration(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jsonToken = tokenHandler.ReadJwtToken(token);

                return jsonToken.ValidTo;
            }
            catch
            {
                return null;
            }
        }
    }
}
