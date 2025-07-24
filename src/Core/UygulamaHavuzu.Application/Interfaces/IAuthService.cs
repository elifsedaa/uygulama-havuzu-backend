using UygulamaHavuzu.Domain.DTOs.Auth;

namespace UygulamaHavuzu.Application.Interfaces
{
	/// <summary>
	/// Authentication (kimlik doðrulama) iþlemleri için service interface'i
	/// Business logic'i Application katmanýnda implement edilecek
	/// </summary>
	public interface IAuthService
	{
		/// <summary>
		/// Kullanýcý giriþ iþlemi
		/// Kullanýcý adý/e-posta ve þifre kontrolü yapar, JWT token üretir
		/// </summary>
		/// <param name="loginDto">Giriþ bilgilerini içeren DTO</param>
		/// <returns>Baþarýlý ise token ve kullanýcý bilgileri, baþarýsýz ise hata mesajý</returns>
		Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginDto loginDto);

		/// <summary>
		/// Kullanýcý kayýt iþlemi
		/// Yeni kullanýcý oluþturur, þifreyi hash'ler, e-posta doðrulama gönderir
		/// </summary>
		/// <param name="registerDto">Kayýt bilgilerini içeren DTO</param>
		/// <returns>Baþarýlý ise kullanýcý bilgileri, baþarýsýz ise hata mesajý</returns>
		Task<ApiResponse<UserInfoDto>> RegisterAsync(RegisterDto registerDto);

		/// <summary>
		/// JWT token'dan kullanýcý bilgilerini çýkarýr
		/// Token geçerliliðini kontrol eder
		/// </summary>
		/// <param name="token">JWT token</param>
		/// <returns>Token geçerli ise kullanýcý bilgileri, deðilse null</returns>
		Task<ApiResponse<UserInfoDto?>> ValidateTokenAsync(string token);

		/// <summary>
		/// Mevcut kullanýcýnýn bilgilerini getirir
		/// Profil sayfasý için kullanýlýr
		/// </summary>
		/// <param name="userId">Kullanýcý ID'si</param>
		/// <returns>Kullanýcý bilgileri</returns>
		Task<ApiResponse<UserInfoDto?>> GetCurrentUserAsync(int userId);

		/// <summary>
		/// E-posta doðrulama iþlemi
		/// Kullanýcýnýn e-posta adresini doðrular
		/// </summary>
		/// <param name="userId">Kullanýcý ID'si</param>
		/// <param name="confirmationToken">Doðrulama token'ý</param>
		/// <returns>Ýþlem sonucu</returns>
		Task<ApiResponse<bool>> ConfirmEmailAsync(int userId, string confirmationToken);

		/// <summary>
		/// Þifre sýfýrlama isteði
		/// Kullanýcýya þifre sýfýrlama linki gönderir
		/// </summary>
		/// <param name="email">E-posta adresi</param>
		/// <returns>Ýþlem sonucu</returns>
		Task<ApiResponse<bool>> ForgotPasswordAsync(string email);

		/// <summary>
		/// Þifre sýfýrlama iþlemi
		/// Þifre sýfýrlama token'ý ile yeni þifre belirlenir
		/// </summary>
		/// <param name="email">E-posta adresi</param>
		/// <param name="resetToken">Þifre sýfýrlama token'ý</param>
		/// <param name="newPassword">Yeni þifre</param>
		/// <returns>Ýþlem sonucu</returns>
		Task<ApiResponse<bool>> ResetPasswordAsync(string email, string resetToken, string newPassword);

		/// <summary>
		/// Kullanýcý çýkýþ iþlemi
		/// Token'ý blacklist'e ekler (opsiyonel)
		/// </summary>
		/// <param name="userId">Kullanýcý ID'si</param>
		/// <param name="token">Geçersiz kýlýnacak token</param>
		/// <returns>Ýþlem sonucu</returns>
		Task<ApiResponse<bool>> LogoutAsync(int userId, string token);
	}

	/// <summary>
	/// JWT token iþlemleri için service interface'i
	/// Token üretme, doðrulama ve yönetim iþlemleri
	/// </summary>
	public interface IJwtTokenService
	{
		/// <summary>
		/// Kullanýcý için JWT token üretir
		/// </summary>
		/// <param name="userId">Kullanýcý ID'si</param>
		/// <param name="username">Kullanýcý adý</param>
		/// <param name="email">E-posta adresi</param>
		/// <param name="role">Kullanýcý rolü</param>
		/// <param name="rememberMe">Beni hatýrla seçeneði (token süresini etkiler)</param>
		/// <returns>JWT token string'i</returns>
		string GenerateToken(int userId, string username, string email, string role, bool rememberMe = false);

		/// <summary>
		/// JWT token'ý doðrular ve içindeki bilgileri çýkarýr
		/// </summary>
		/// <param name="token">Doðrulanacak JWT token</param>
		/// <returns>Token geçerli ise kullanýcý ID'si, deðilse null</returns>
		int? ValidateToken(string token);

		/// <summary>
		/// JWT token'ýn süresinin dolup dolmadýðýný kontrol eder
		/// </summary>
		/// <param name="token">Kontrol edilecek token</param>
		/// <returns>Token geçerli ise true</returns>
		bool IsTokenExpired(string token);

		/// <summary>
		/// Token'dan kullanýcý bilgilerini çýkarýr
		/// </summary>
		/// <param name="token">JWT token</param>
		/// <returns>Kullanýcý bilgileri (Dictionary formatýnda)</returns>
		Dictionary<string, string>? GetTokenClaims(string token);

		/// <summary>
		/// Token'ýn ne zaman sona ereceðini döndürür
		/// </summary>
		/// <param name="token">JWT token</param>
		/// <returns>Sona erme tarihi</returns>
		DateTime? GetTokenExpiration(string token);
	}

	/// <summary>
	/// Þifre iþlemleri için service interface'i
	/// Þifre hash'leme, doðrulama ve güvenlik iþlemleri
	/// </summary>
	public interface IPasswordService
	{
		/// <summary>
		/// Düz metinli þifreyi hash'ler
		/// Kayýt ve þifre deðiþtirme iþlemlerinde kullanýlýr
		/// </summary>
		/// <param name="password">Düz metinli þifre</param>
		/// <returns>Hash'lenmiþ þifre</returns>
		string HashPassword(string password);

		/// <summary>
		/// Düz metinli þifre ile hash'lenmiþ þifreyi karþýlaþtýrýr
		/// Login iþlemlerinde kullanýlýr
		/// </summary>
		/// <param name="password">Düz metinli þifre</param>
		/// <param name="hashedPassword">Hash'lenmiþ þifre</param>
		/// <returns>Þifreler eþleþiyorsa true</returns>
		bool VerifyPassword(string password, string hashedPassword);

		/// <summary>
		/// Güvenli rastgele þifre üretir
		/// Geçici þifreler için kullanýlýr
		/// </summary>
		/// <param name="length">Þifre uzunluðu</param>
		/// <param name="includeSpecialChars">Özel karakter içersin mi</param>
		/// <returns>Rastgele þifre</returns>
		string GenerateRandomPassword(int length = 12, bool includeSpecialChars = true);

		/// <summary>
		/// Þifrenin güvenlik kurallarýna uygun olup olmadýðýný kontrol eder
		/// </summary>
		/// <param name="password">Kontrol edilecek þifre</param>
		/// <returns>Þifre geçerli ise true, geçersiz ise false ve hata mesajlarý</returns>
		(bool IsValid, List<string> Errors) ValidatePassword(string password);
	}
}