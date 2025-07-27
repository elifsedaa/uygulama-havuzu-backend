using UygulamaHavuzu.Application.Interfaces;
using UygulamaHavuzu.Domain.DTOs.Auth;
using UygulamaHavuzu.Domain.Entities;
using UygulamaHavuzu.Domain.Interfaces;

namespace UygulamaHavuzu.Application.Services
{
    /// <summary>
    /// Authentication i�lemlerinin business logic'ini i�eren service
    /// Clean Architecture'da Application katman�nda yer al�r
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
        /// Kullan�c� giri� i�lemi
        /// 1. Kullan�c�y� bul (username veya email ile)
        /// 2. �ifre kontrol� yap
        /// 3. Kullan�c� aktif mi kontrol et
        /// 4. JWT token �ret
        /// 5. Son giri� tarihini g�ncelle
        /// </summary>
        public async Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginDto loginDto)
        {
            try
            {
                // 1. Kullan�c�y� bul
                var user = await _userRepository.GetByUsernameOrEmailAsync(loginDto.UsernameOrEmail);
                if (user == null)
                {
                    return ApiResponse<LoginResponseDto>.ErrorResult(
                        "Kullan�c� ad�/e-posta veya �ifre hatal�");
                }

                // 2. �ifre kontrol�
                if (!_passwordService.VerifyPassword(loginDto.Password, user.PasswordHash))
                {
                    return ApiResponse<LoginResponseDto>.ErrorResult(
                        "Kullan�c� ad�/e-posta veya �ifre hatal�");
                }

                // 3. Kullan�c� aktif mi kontrol et
                if (!user.IsActive)
                {
                    return ApiResponse<LoginResponseDto>.ErrorResult(
                        "Hesab�n�z devre d��� b�rak�lm��. L�tfen y�netici ile ileti�ime ge�in.");
                }

                // 4. JWT token �ret
                var token = _jwtTokenService.GenerateToken(
                    user.Id,
                    user.Username,
                    user.Email,
                    user.Role,
                    loginDto.RememberMe);

                // Token'�n sona erme tarihini al
                var tokenExpiration = _jwtTokenService.GetTokenExpiration(token);

                // 5. Son giri� tarihini g�ncelle
                await _userRepository.UpdateLastLoginAsync(user.Id);

                // Ba�ar�l� yan�t olu�tur
                var response = new LoginResponseDto
                {
                    Token = token,
                    ExpiresAt = tokenExpiration ?? DateTime.UtcNow.AddHours(1), // fallback
                    User = MapToUserInfoDto(user)
                };

                return ApiResponse<LoginResponseDto>.SuccessResult(
                    response,
                    "Giri� ba�ar�l�. Ho� geldiniz!");
            }
            catch (Exception ex)
            {
                // Log the exception (implement logging later)
                return ApiResponse<LoginResponseDto>.ErrorResult(
                    "Giri� i�lemi s�ras�nda bir hata olu�tu. L�tfen tekrar deneyin.");
            }
        }

        /// <summary>
        /// Kullan�c� kay�t i�lemi
        /// 1. Validation kontrolleri
        /// 2. Username ve email benzersizlik kontrol�
        /// 3. �ifreyi hash'le
        /// 4. Kullan�c�y� olu�tur
        /// 5. E-posta do�rulama g�nder (implement later)
        /// </summary>
        public async Task<ApiResponse<UserInfoDto>> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                // 1. Username benzersizlik kontrol�
                if (!await _userRepository.IsUsernameUniqueAsync(registerDto.Username))
                {
                    return ApiResponse<UserInfoDto>.ErrorResult(
                        "Bu kullan�c� ad� zaten kullan�l�yor.");
                }

                // 2. Email benzersizlik kontrol�
                if (!await _userRepository.IsEmailUniqueAsync(registerDto.Email))
                {
                    return ApiResponse<UserInfoDto>.ErrorResult(
                        "Bu e-posta adresi zaten kullan�l�yor.");
                }

                // 3. �ifre validation
                var passwordValidation = _passwordService.ValidatePassword(registerDto.Password);
                if (!passwordValidation.IsValid)
                {
                    return ApiResponse<UserInfoDto>.ErrorResult(
                        "�ifre g�venlik gereksinimlerini kar��lam�yor.",
                        passwordValidation.Errors);
                }

                // 4. �ifreyi hash'le
                var hashedPassword = _passwordService.HashPassword(registerDto.Password);

                // 5. User entity olu�tur
                var user = new User
                {
                    Username = registerDto.Username,
                    Email = registerDto.Email,
                    PasswordHash = hashedPassword,
                    FullName = registerDto.FullName,
                    IsActive = true,
                    EmailConfirmed = false, // E-posta do�rulama yap�lacak
                    CreatedAt = DateTime.UtcNow,
                    Role = "User" // Varsay�lan rol
                };

                // 6. Veritaban�na kaydet
                var createdUser = await _userRepository.CreateAsync(user);

                // 7. TODO: E-posta do�rulama g�nder
                // await _emailService.SendEmailConfirmationAsync(createdUser.Email, confirmationToken);

                return ApiResponse<UserInfoDto>.SuccessResult(
                    MapToUserInfoDto(createdUser),
                    "Kay�t ba�ar�l�! E-posta adresinize do�rulama linki g�nderildi.");
            }
            catch (Exception ex)
            {
                // Log the exception
                return ApiResponse<UserInfoDto>.ErrorResult(
                    "Kay�t i�lemi s�ras�nda bir hata olu�tu. L�tfen tekrar deneyin.");
            }
        }

        /// <summary>
        /// JWT token do�rulama
        /// Middleware'de kullan�l�r
        /// </summary>
        public async Task<ApiResponse<UserInfoDto?>> ValidateTokenAsync(string token)
        {
            try
            {
                // Token ge�erlili�ini kontrol et
                var userId = _jwtTokenService.ValidateToken(token);
                if (userId == null)
                {
                    return ApiResponse<UserInfoDto?>.ErrorResult("Ge�ersiz token");
                }

                // Kullan�c�y� veritaban�ndan getir
                var user = await _userRepository.GetByIdAsync(userId.Value);
                if (user == null || !user.IsActive)
                {
                    return ApiResponse<UserInfoDto?>.ErrorResult("Kullan�c� bulunamad� veya aktif de�il");
                }

                return ApiResponse<UserInfoDto?>.SuccessResult(MapToUserInfoDto(user));
            }
            catch (Exception ex)
            {
                return ApiResponse<UserInfoDto?>.ErrorResult("Token do�rulama hatas�");
            }
        }

        /// <summary>
        /// Mevcut kullan�c� bilgilerini getir
        /// </summary>
        public async Task<ApiResponse<UserInfoDto?>> GetCurrentUserAsync(int userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return ApiResponse<UserInfoDto?>.ErrorResult("Kullan�c� bulunamad�");
                }

                return ApiResponse<UserInfoDto?>.SuccessResult(MapToUserInfoDto(user));
            }
            catch (Exception ex)
            {
                return ApiResponse<UserInfoDto?>.ErrorResult("Kullan�c� bilgileri al�n�rken hata olu�tu");
            }
        }

        /// <summary>
        /// E-posta do�rulama - �imdilik basit implementasyon
        /// </summary>
        public async Task<ApiResponse<bool>> ConfirmEmailAsync(int userId, string confirmationToken)
        {
            try
            {
                // TODO: Token do�rulama logic'i implement et
                var result = await _userRepository.UpdateEmailConfirmationAsync(userId, true);

                if (result)
                {
                    return ApiResponse<bool>.SuccessResult(true, "E-posta adresiniz ba�ar�yla do�ruland�");
                }

                return ApiResponse<bool>.ErrorResult("E-posta do�rulama ba�ar�s�z");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult("E-posta do�rulama s�ras�nda hata olu�tu");
            }
        }

        /// <summary>
        /// �ifre s�f�rlama iste�i - TODO: implement
        /// </summary>
        public async Task<ApiResponse<bool>> ForgotPasswordAsync(string email)
        {
            // TODO: Implement forgot password logic
            return ApiResponse<bool>.ErrorResult("Bu �zellik hen�z implement edilmedi");
        }

        /// <summary>
        /// �ifre s�f�rlama - TODO: implement
        /// </summary>
        public async Task<ApiResponse<bool>> ResetPasswordAsync(string email, string resetToken, string newPassword)
        {
            // TODO: Implement reset password logic
            return ApiResponse<bool>.ErrorResult("Bu �zellik hen�z implement edilmedi");
        }

        /// <summary>
        /// Kullan�c� ��k�� i�lemi - �imdilik basit implementasyon
        /// </summary>
        public async Task<ApiResponse<bool>> LogoutAsync(int userId, string token)
        {
            try
            {
                // TODO: Token blacklist logic implement et
                return ApiResponse<bool>.SuccessResult(true, "Ba�ar�yla ��k�� yap�ld�");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult("��k�� i�lemi s�ras�nda hata olu�tu");
            }
        }

        /// <summary>
        /// User entity'sini UserInfoDto'ya �eviren helper method
        /// G�venlik a��s�ndan hassas bilgiler (password hash) dahil edilmez
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
    

