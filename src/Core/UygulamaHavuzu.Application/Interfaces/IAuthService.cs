using UygulamaHavuzu.Domain.DTOs.Auth;

namespace UygulamaHavuzu.Application.Interfaces
{
	/// <summary>
	/// Authentication (kimlik do�rulama) i�lemleri i�in service interface'i
	/// Business logic'i Application katman�nda implement edilecek
	/// </summary>
	public interface IAuthService
	{
		/// <summary>
		/// Kullan�c� giri� i�lemi
		/// Kullan�c� ad�/e-posta ve �ifre kontrol� yapar, JWT token �retir
		/// </summary>
		/// <param name="loginDto">Giri� bilgilerini i�eren DTO</param>
		/// <returns>Ba�ar�l� ise token ve kullan�c� bilgileri, ba�ar�s�z ise hata mesaj�</returns>
		Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginDto loginDto);

		/// <summary>
		/// Kullan�c� kay�t i�lemi
		/// Yeni kullan�c� olu�turur, �ifreyi hash'ler, e-posta do�rulama g�nderir
		/// </summary>
		/// <param name="registerDto">Kay�t bilgilerini i�eren DTO</param>
		/// <returns>Ba�ar�l� ise kullan�c� bilgileri, ba�ar�s�z ise hata mesaj�</returns>
		Task<ApiResponse<UserInfoDto>> RegisterAsync(RegisterDto registerDto);

		/// <summary>
		/// JWT token'dan kullan�c� bilgilerini ��kar�r
		/// Token ge�erlili�ini kontrol eder
		/// </summary>
		/// <param name="token">JWT token</param>
		/// <returns>Token ge�erli ise kullan�c� bilgileri, de�ilse null</returns>
		Task<ApiResponse<UserInfoDto?>> ValidateTokenAsync(string token);

		/// <summary>
		/// Mevcut kullan�c�n�n bilgilerini getirir
		/// Profil sayfas� i�in kullan�l�r
		/// </summary>
		/// <param name="userId">Kullan�c� ID'si</param>
		/// <returns>Kullan�c� bilgileri</returns>
		Task<ApiResponse<UserInfoDto?>> GetCurrentUserAsync(int userId);

		/// <summary>
		/// E-posta do�rulama i�lemi
		/// Kullan�c�n�n e-posta adresini do�rular
		/// </summary>
		/// <param name="userId">Kullan�c� ID'si</param>
		/// <param name="confirmationToken">Do�rulama token'�</param>
		/// <returns>��lem sonucu</returns>
		Task<ApiResponse<bool>> ConfirmEmailAsync(int userId, string confirmationToken);

		/// <summary>
		/// �ifre s�f�rlama iste�i
		/// Kullan�c�ya �ifre s�f�rlama linki g�nderir
		/// </summary>
		/// <param name="email">E-posta adresi</param>
		/// <returns>��lem sonucu</returns>
		Task<ApiResponse<bool>> ForgotPasswordAsync(string email);

		/// <summary>
		/// �ifre s�f�rlama i�lemi
		/// �ifre s�f�rlama token'� ile yeni �ifre belirlenir
		/// </summary>
		/// <param name="email">E-posta adresi</param>
		/// <param name="resetToken">�ifre s�f�rlama token'�</param>
		/// <param name="newPassword">Yeni �ifre</param>
		/// <returns>��lem sonucu</returns>
		Task<ApiResponse<bool>> ResetPasswordAsync(string email, string resetToken, string newPassword);

		/// <summary>
		/// Kullan�c� ��k�� i�lemi
		/// Token'� blacklist'e ekler (opsiyonel)
		/// </summary>
		/// <param name="userId">Kullan�c� ID'si</param>
		/// <param name="token">Ge�ersiz k�l�nacak token</param>
		/// <returns>��lem sonucu</returns>
		Task<ApiResponse<bool>> LogoutAsync(int userId, string token);
	}

	/// <summary>
	/// JWT token i�lemleri i�in service interface'i
	/// Token �retme, do�rulama ve y�netim i�lemleri
	/// </summary>
	public interface IJwtTokenService
	{
		/// <summary>
		/// Kullan�c� i�in JWT token �retir
		/// </summary>
		/// <param name="userId">Kullan�c� ID'si</param>
		/// <param name="username">Kullan�c� ad�</param>
		/// <param name="email">E-posta adresi</param>
		/// <param name="role">Kullan�c� rol�</param>
		/// <param name="rememberMe">Beni hat�rla se�ene�i (token s�resini etkiler)</param>
		/// <returns>JWT token string'i</returns>
		string GenerateToken(int userId, string username, string email, string role, bool rememberMe = false);

		/// <summary>
		/// JWT token'� do�rular ve i�indeki bilgileri ��kar�r
		/// </summary>
		/// <param name="token">Do�rulanacak JWT token</param>
		/// <returns>Token ge�erli ise kullan�c� ID'si, de�ilse null</returns>
		int? ValidateToken(string token);

		/// <summary>
		/// JWT token'�n s�resinin dolup dolmad���n� kontrol eder
		/// </summary>
		/// <param name="token">Kontrol edilecek token</param>
		/// <returns>Token ge�erli ise true</returns>
		bool IsTokenExpired(string token);

		/// <summary>
		/// Token'dan kullan�c� bilgilerini ��kar�r
		/// </summary>
		/// <param name="token">JWT token</param>
		/// <returns>Kullan�c� bilgileri (Dictionary format�nda)</returns>
		Dictionary<string, string>? GetTokenClaims(string token);

		/// <summary>
		/// Token'�n ne zaman sona erece�ini d�nd�r�r
		/// </summary>
		/// <param name="token">JWT token</param>
		/// <returns>Sona erme tarihi</returns>
		DateTime? GetTokenExpiration(string token);
	}

	/// <summary>
	/// �ifre i�lemleri i�in service interface'i
	/// �ifre hash'leme, do�rulama ve g�venlik i�lemleri
	/// </summary>
	public interface IPasswordService
	{
		/// <summary>
		/// D�z metinli �ifreyi hash'ler
		/// Kay�t ve �ifre de�i�tirme i�lemlerinde kullan�l�r
		/// </summary>
		/// <param name="password">D�z metinli �ifre</param>
		/// <returns>Hash'lenmi� �ifre</returns>
		string HashPassword(string password);

		/// <summary>
		/// D�z metinli �ifre ile hash'lenmi� �ifreyi kar��la�t�r�r
		/// Login i�lemlerinde kullan�l�r
		/// </summary>
		/// <param name="password">D�z metinli �ifre</param>
		/// <param name="hashedPassword">Hash'lenmi� �ifre</param>
		/// <returns>�ifreler e�le�iyorsa true</returns>
		bool VerifyPassword(string password, string hashedPassword);

		/// <summary>
		/// G�venli rastgele �ifre �retir
		/// Ge�ici �ifreler i�in kullan�l�r
		/// </summary>
		/// <param name="length">�ifre uzunlu�u</param>
		/// <param name="includeSpecialChars">�zel karakter i�ersin mi</param>
		/// <returns>Rastgele �ifre</returns>
		string GenerateRandomPassword(int length = 12, bool includeSpecialChars = true);

		/// <summary>
		/// �ifrenin g�venlik kurallar�na uygun olup olmad���n� kontrol eder
		/// </summary>
		/// <param name="password">Kontrol edilecek �ifre</param>
		/// <returns>�ifre ge�erli ise true, ge�ersiz ise false ve hata mesajlar�</returns>
		(bool IsValid, List<string> Errors) ValidatePassword(string password);
	}
}