using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UygulamaHavuzu.Application.Interfaces;

namespace UygulamaHavuzu.Application.Services
{
    // JWT iþlemlerini yöneten servis sýnýfý
    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _configuration;
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _normalExpirationMinutes;
        private readonly int _rememberMeExpirationDays;

        // Constructor: appsettings.json dosyasýndan JWT ayarlarýný okur
        public JwtTokenService(IConfiguration configuration)
        {
            _configuration = configuration;

            // Secret key, issuer ve audience deðerlerini al
            _secretKey = _configuration["JWT:SecretKey"] ?? throw new ArgumentNullException("JWT:SecretKey configuration is missing");
            _issuer = _configuration["JWT:Issuer"] ?? "UygulamaHavuzu";
            _audience = _configuration["JWT:Audience"] ?? "UygulamaHavuzu-Users";

            // Token süresi ayarlarýný al
            _normalExpirationMinutes = int.Parse(_configuration["JWT:ExpirationMinutes"] ?? "60");
            _rememberMeExpirationDays = int.Parse(_configuration["JWT:RememberMeExpirationDays"] ?? "30");
        }

        // Kullanýcý bilgilerine göre JWT token üretir
        public string GenerateToken(int userId, string username, string email, string role, bool rememberMe = false)
        {
            try
            {
                // Güvenlik anahtarý ve imza oluþtur
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                // Token içine eklenecek claim'ler
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

                // Token geçerlilik süresi belirlenir
                var expiration = rememberMe
                    ? DateTime.UtcNow.AddDays(_rememberMeExpirationDays)
                    : DateTime.UtcNow.AddMinutes(_normalExpirationMinutes);

                // JWT token nesnesi oluþturulur
                var token = new JwtSecurityToken(
                    issuer: _issuer,
                    audience: _audience,
                    claims: claims,
                    expires: expiration,
                    signingCredentials: credentials
                );

                // Token string olarak döndürülür
                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Token oluþturulurken hata oluþtu", ex);
            }
        }

        // Token'ý doðrular, geçerliyse kullanýcý ID'sini döner
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

                // Token doðrulama iþlemi yapýlýr
                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);

                // Kullanýcý ID claim'i alýnýr
                var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier) ?? principal.FindFirst("userId");

                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    return userId;
                }

                return null;
            }
            catch (SecurityTokenExpiredException)
            {
                // Token süresi dolmuþsa null döner
                return null;
            }
            catch (SecurityTokenException)
            {
                // Geçersiz token
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        // Token'ýn süresinin dolup dolmadýðýný kontrol eder
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
                // Hatalý token varsa süresi dolmuþ kabul edilir
                return true;
            }
        }

        // Token içindeki tüm claim'leri dictionary olarak döner
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

        // Token'ýn geçerlilik bitiþ zamanýný döner
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
