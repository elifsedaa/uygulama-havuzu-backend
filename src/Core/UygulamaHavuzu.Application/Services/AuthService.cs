using UygulamaHavuzu.Application.Interfaces;
using UygulamaHavuzu.Domain.DTOs.Auth;
using UygulamaHavuzu.Domain.Entities;
using UygulamaHavuzu.Domain.Interfaces;

namespace UygulamaHavuzu.Application.Services
{
    /// <summary>
    /// Authentication iþlemlerinin business logic'ini içeren service
    /// Clean Architecture'da Application katmanýnda yer alýr
    /// </summary>
    public class AuthService : IAuthService
    {
        // Dependency Injection ile gelen servisler
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IPasswordService _passwordService;

        /// <summary>
        /// Constructor - Dependency Injection
        /// </summary>
        public AuthService(
            IUserRepository userRepository,
            IJwtTokenService jwtTokenService,
            IPasswordService passwordService)
        {
            _userRepository = userRepository;
            _jwtTokenService = jwtTokenService;
            _passwordService = passwordService;
        }

        /// <summary>
        /// Kullanýcý giriþ iþlemi
        /// 1. Kullanýcýyý bul (username veya email ile)
        /// 2. Þifre kontrolü yap
        /// 3. Kullanýcý aktif mi kontrol et
        /// 4. JWT token üret
        /// 5. Son giriþ tarihini güncelle
        /// </summary>
        public async Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginDto loginDto)
        {
            try
            {
                // 1. Kullanýcýyý bul
                var user = await _userRepository.GetByUsernameOrEmailAsync(loginDto.UsernameOrEmail);
                if (user == null)
                {
                    return ApiResponse<LoginResponseDto>.ErrorResult(
                        "Kullanýcý adý/e-posta veya þifre hatalý");
                }

                // 2. Þifre kontrolü
                if (!_passwordService.VerifyPassword(loginDto.Password, user.PasswordHash))
                {
                    return ApiResponse<LoginResponseDto>.ErrorResult(
                        "Kullanýcý adý/e-posta veya þifre hatalý");
                }

                // 3. Kullanýcý aktif mi kontrol et
                if (!user.IsActive)
                {
                    return ApiResponse<LoginResponseDto>.ErrorResult(
                        "Hesabýnýz devre dýþý býrakýlmýþ. Lütfen yönetici ile iletiþime geçin.");
                }

                // 4. JWT token üret
                var token = _jwtTokenService.GenerateToken(
                    user.Id,
                    user.Username,
                    user.Email,
                    user.Role,
                    loginDto.RememberMe);

                // Token'ýn sona erme tarihini al
                var tokenExpiration = _jwtTokenService.GetTokenExpiration(token);

                // 5. Son giriþ tarihini güncelle
                await _userRepository.UpdateLastLoginAsync(user.Id);

                // Baþarýlý yanýt oluþtur
                var response = new LoginResponseDto
                {
                    Token = token,
                    ExpiresAt = tokenExpiration ?? DateTime.UtcNow.AddHours(1), // fallback
                    User = MapToUserInfoDto(user)
                };

                return ApiResponse<LoginResponseDto>.SuccessResult(
                    response,
                    "Giriþ baþarýlý. Hoþ geldiniz!");
            }
            catch (Exception ex)
            {
                // Log the exception (implement logging later)
                return ApiResponse<LoginResponseDto>.ErrorResult(
                    "Giriþ iþlemi sýrasýnda bir hata oluþtu. Lütfen tekrar deneyin.");
            }
        }

        /// <summary>
        /// Kullanýcý kayýt iþlemi
        /// 1. Validation kontrolleri
        /// 2. Username ve email benzersizlik kontrolü
        /// 3. Þifreyi hash'le
        /// 4. Kullanýcýyý oluþtur
        /// 5. E-posta doðrulama gönder (implement later)
        /// </summary>
        public async Task<ApiResponse<UserInfoDto>> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                // 1. Username benzersizlik kontrolü
                if (!await _userRepository.IsUsernameUniqueAsync(registerDto.Username))
                {
                    return ApiResponse<UserInfoDto>.ErrorResult(
                        "Bu kullanýcý adý zaten kullanýlýyor.");
                }

                // 2. Email benzersizlik kontrolü
                if (!await _userRepository.IsEmailUniqueAsync(registerDto.Email))
                {
                    return ApiResponse<UserInfoDto>.ErrorResult(
                        "Bu e-posta adresi zaten kullanýlýyor.");
                }

                // 3. Þifre validation
                var passwordValidation = _passwordService.ValidatePassword(registerDto.Password);
                if (!passwordValidation.IsValid)
                {
                    return ApiResponse<UserInfoDto>.ErrorResult(
                        "Þifre güvenlik gereksinimlerini karþýlamýyor.",
                        passwordValidation.Errors);
                }

                // 4. Þifreyi hash'le
                var hashedPassword = _passwordService.HashPassword(registerDto.Password);

                // 5. User entity oluþtur
                var user = new User
                {
                    Username = registerDto.Username,
                    Email = registerDto.Email,
                    PasswordHash = hashedPassword,
                    FullName = registerDto.FullName,
                    IsActive = true,
                    EmailConfirmed = false, // E-posta doðrulama yapýlacak
                    CreatedAt = DateTime.UtcNow,
                    Role = "User" // Varsayýlan rol
                };

                // 6. Veritabanýna kaydet
                var createdUser = await _userRepository.CreateAsync(user);

                // 7. TODO: E-posta doðrulama gönder
                // await _emailService.SendEmailConfirmationAsync(createdUser.Email, confirmationToken);

                return ApiResponse<UserInfoDto>.SuccessResult(
                    MapToUserInfoDto(createdUser),
                    "Kayýt baþarýlý! E-posta adresinize doðrulama linki gönderildi.");
            }
            catch (Exception ex)
            {
                // Log the exception
                return ApiResponse<UserInfoDto>.ErrorResult(
                    "Kayýt iþlemi sýrasýnda bir hata oluþtu. Lütfen tekrar deneyin.");
            }
        }

        /// <summary>
        /// JWT token doðrulama
        /// Middleware'de kullanýlýr
        /// </summary>
        public async Task<ApiResponse<UserInfoDto?>> ValidateTokenAsync(string token)
        {
            try
            {
                // Token geçerliliðini kontrol et
                var userId = _jwtTokenService.ValidateToken(token);
                if (userId == null)
                {
                    return ApiResponse<UserInfoDto?>.ErrorResult("Geçersiz token");
                }

                // Kullanýcýyý veritabanýndan getir
                var user = await _userRepository.GetByIdAsync(userId.Value);
                if (user == null || !user.IsActive)
                {
                    return ApiResponse<UserInfoDto?>.ErrorResult("Kullanýcý bulunamadý veya aktif deðil");
                }

                return ApiResponse<UserInfoDto?>.SuccessResult(MapToUserInfoDto(user));
            }
            catch (Exception ex)
            {
                return ApiResponse<UserInfoDto?>.ErrorResult("Token doðrulama hatasý");
            }
        }

        /// <summary>
        /// Mevcut kullanýcý bilgilerini getir
        /// </summary>
        public async Task<ApiResponse<UserInfoDto?>> GetCurrentUserAsync(int userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return ApiResponse<UserInfoDto?>.ErrorResult("Kullanýcý bulunamadý");
                }

                return ApiResponse<UserInfoDto?>.SuccessResult(MapToUserInfoDto(user));
            }
            catch (Exception ex)
            {
                return ApiResponse<UserInfoDto?>.ErrorResult("Kullanýcý bilgileri alýnýrken hata oluþtu");
            }
        }

        /// <summary>
        /// E-posta doðrulama - þimdilik basit implementasyon
        /// </summary>
        public async Task<ApiResponse<bool>> ConfirmEmailAsync(int userId, string confirmationToken)
        {
            try
            {
                // TODO: Token doðrulama logic'i implement et
                var result = await _userRepository.UpdateEmailConfirmationAsync(userId, true);

                if (result)
                {
                    return ApiResponse<bool>.SuccessResult(true, "E-posta adresiniz baþarýyla doðrulandý");
                }

                return ApiResponse<bool>.ErrorResult("E-posta doðrulama baþarýsýz");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult("E-posta doðrulama sýrasýnda hata oluþtu");
            }
        }

        /// <summary>
        /// Þifre sýfýrlama isteði - TODO: implement
        /// </summary>
        public async Task<ApiResponse<bool>> ForgotPasswordAsync(string email)
        {
            // TODO: Implement forgot password logic
            return ApiResponse<bool>.ErrorResult("Bu özellik henüz implement edilmedi");
        }

        /// <summary>
        /// Þifre sýfýrlama - TODO: implement
        /// </summary>
        public async Task<ApiResponse<bool>> ResetPasswordAsync(string email, string resetToken, string newPassword)
        {
            // TODO: Implement reset password logic
            return ApiResponse<bool>.ErrorResult("Bu özellik henüz implement edilmedi");
        }

        /// <summary>
        /// Kullanýcý çýkýþ iþlemi - þimdilik basit implementasyon
        /// </summary>
        public async Task<ApiResponse<bool>> LogoutAsync(int userId, string token)
        {
            try
            {
                // TODO: Token blacklist logic implement et
                return ApiResponse<bool>.SuccessResult(true, "Baþarýyla çýkýþ yapýldý");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult("Çýkýþ iþlemi sýrasýnda hata oluþtu");
            }
        }

        /// <summary>
        /// User entity'sini UserInfoDto'ya çeviren helper method
        /// Güvenlik açýsýndan hassas bilgiler (password hash) dahil edilmez
        /// </summary>
        private UserInfoDto MapToUserInfoDto(User user)
        {
            return new UserInfoDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role,
                EmailConfirmed = user.EmailConfirmed,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            };
        }
    }
}
    

