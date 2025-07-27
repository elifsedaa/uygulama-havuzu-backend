using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using UygulamaHavuzu.Application.Interfaces;
using UygulamaHavuzu.Domain.DTOs.Auth;

namespace UygulamaHavuzu.WebAPI.Controllers
{
    // Authentication işlemleri için API Controller
    // Login, Register, profil bilgileri gibi işlemleri yönetir
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        // Constructor - Dependency Injection ile AuthService alır
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // POST: api/auth/login
        // Kullanıcı giriş işlemi
        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginDto loginDto)
        {
            // Model validation kontrolü
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResponse<LoginResponseDto>.ErrorResult(
                    "Giriş bilgileri geçersiz", errors));
            }

            // AuthService ile login işlemini gerçekleştir
            var result = await _authService.LoginAsync(loginDto);

            // Sonuca göre HTTP status code döndür
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        // POST: api/auth/register
        // Kullanıcı kayıt işlemi
        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<UserInfoDto>>> Register([FromBody] RegisterDto registerDto)
        {
            // Model validation kontrolü
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResponse<UserInfoDto>.ErrorResult(
                    "Kayıt bilgileri geçersiz", errors));
            }

            // AuthService ile kayıt işlemini gerçekleştir
            var result = await _authService.RegisterAsync(registerDto);

            // Sonuca göre HTTP status code döndür
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        // GET: api/auth/me
        // Mevcut kullanıcının bilgilerini getirir
        // Authorization header'dan JWT token okunur
        [HttpGet("me")]
        [Authorize] // Bu endpoint için authentication gerekli
        public async Task<ActionResult<ApiResponse<UserInfoDto?>>> GetCurrentUser()
        {
            // JWT token'dan kullanıcı ID'sini çıkar
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(ApiResponse<UserInfoDto?>.ErrorResult("Geçersiz token"));
            }

            // AuthService ile kullanıcı bilgilerini getir
            var result = await _authService.GetCurrentUserAsync(userId);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        // POST: api/auth/logout
        // Kullanıcı çıkış işlemi
        [HttpPost("logout")]
        [Authorize] // Bu endpoint için authentication gerekli
        public async Task<ActionResult<ApiResponse<bool>>> Logout()
        {
            // JWT token'dan kullanıcı ID'sini çıkar
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(ApiResponse<bool>.ErrorResult("Geçersiz token"));
            }

            // Authorization header'dan token'ı al
            var token = Request.Headers["Authorization"]
                .FirstOrDefault()?.Split(" ").Last();

            if (string.IsNullOrEmpty(token))
            {
                return BadRequest(ApiResponse<bool>.ErrorResult("Token bulunamadı"));
            }

            // AuthService ile logout işlemini gerçekleştir
            var result = await _authService.LogoutAsync(userId, token);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        // POST: api/auth/confirm-email
        // E-posta doğrulama işlemi
        [HttpPost("confirm-email")]
        public async Task<ActionResult<ApiResponse<bool>>> ConfirmEmail([FromQuery] int userId, [FromQuery] string token)
        {
            // Parameter validation
            if (userId <= 0 || string.IsNullOrEmpty(token))
            {
                return BadRequest(ApiResponse<bool>.ErrorResult("Geçersiz doğrulama parametreleri"));
            }

            // AuthService ile e-posta doğrulama işlemini gerçekleştir
            var result = await _authService.ConfirmEmailAsync(userId, token);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        // POST: api/auth/forgot-password
        // Şifre sıfırlama isteği
        [HttpPost("forgot-password")]
        public async Task<ActionResult<ApiResponse<bool>>> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
        {
            // Model validation kontrolü
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<bool>.ErrorResult("E-posta adresi geçersiz"));
            }

            // AuthService ile şifre sıfırlama isteği işlemini gerçekleştir
            var result = await _authService.ForgotPasswordAsync(forgotPasswordDto.Email);

            // Güvenlik için her durumda başarılı mesaj döndür
            // Bu sayede saldırganlar sistemde hangi e-postaların kayıtlı olduğunu öğrenemez
            return Ok(ApiResponse<bool>.SuccessResult(true,
                "Eğer bu e-posta adresi sistemde kayıtlı ise, şifre sıfırlama linki gönderildi"));
        }

        // POST: api/auth/reset-password
        // Şifre sıfırlama işlemi
        [HttpPost("reset-password")]
        public async Task<ActionResult<ApiResponse<bool>>> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            // Model validation kontrolü
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResponse<bool>.ErrorResult(
                    "Şifre sıfırlama bilgileri geçersiz", errors));
            }

            // AuthService ile şifre sıfırlama işlemini gerçekleştir
            var result = await _authService.ResetPasswordAsync(
                resetPasswordDto.Email,
                resetPasswordDto.Token,
                resetPasswordDto.NewPassword);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        // GET: api/auth/validate-token
        // Token doğrulama endpoint'i (debug/test amaçlı)
        [HttpGet("validate-token")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<UserInfoDto?>>> ValidateToken()
        {
            // Authorization header'dan token'ı al
            var token = Request.Headers["Authorization"]
                .FirstOrDefault()?.Split(" ").Last();

            if (string.IsNullOrEmpty(token))
            {
                return BadRequest(ApiResponse<UserInfoDto?>.ErrorResult("Token bulunamadı"));
            }

            // AuthService ile token doğrulama işlemini gerçekleştir
            var result = await _authService.ValidateTokenAsync(token);

            if (result.Success)
            {
                return Ok(result);
            }

            return Unauthorized(result);
        }
    }

    // Şifre sıfırlama isteği için DTO
    public class ForgotPasswordDto
    {
        [Required(ErrorMessage = "E-posta adresi zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        public string Email { get; set; } = string.Empty;
    }

    // Şifre sıfırlama işlemi için DTO
    public class ResetPasswordDto
    {
        [Required(ErrorMessage = "E-posta adresi zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Sıfırlama token'ı zorunludur")]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "Yeni şifre zorunludur")]
        [MinLength(8, ErrorMessage = "Şifre en az 8 karakter olmalıdır")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre doğrulama zorunludur")]
        [Compare("NewPassword", ErrorMessage = "Şifreler eşleşmiyor")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}