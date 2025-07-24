using UygulamaHavuzu.Domain.Entities;

namespace UygulamaHavuzu.Domain.Interfaces
{
    /// <summary>
    /// Kullan�c� veri eri�im i�lemleri i�in interface
    /// Clean Architecture'da Domain katman� Infrastructure'dan ba��ms�z olmal�
    /// Bu interface Persistence katman�nda implement edilecek
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Kullan�c� ID'sine g�re kullan�c� getirir
        /// </summary>
        /// <param name="id">Kullan�c� ID'si</param>
        /// <returns>Kullan�c� varsa User entity, yoksa null</returns>
        Task<User?> GetByIdAsync(int id);

        /// <summary>
        /// Kullan�c� ad�na g�re kullan�c� getirir
        /// Login i�lemlerinde kullan�l�r
        /// </summary>
        /// <param name="username">Kullan�c� ad�</param>
        /// <returns>Kullan�c� varsa User entity, yoksa null</returns>
        Task<User?> GetByUsernameAsync(string username);

        /// <summary>
        /// E-posta adresine g�re kullan�c� getirir
        /// Login ve e-posta do�rulama i�lemlerinde kullan�l�r
        /// </summary>
        /// <param name="email">E-posta adresi</param>
        /// <returns>Kullan�c� varsa User entity, yoksa null</returns>
        Task<User?> GetByEmailAsync(string email);

        /// <summary>
        /// Kullan�c� ad� veya e-posta ile kullan�c� getirir
        /// Login i�lemlerinde kullan�c� hem username hem email ile giri� yapabilir
        /// </summary>
        /// <param name="usernameOrEmail">Kullan�c� ad� veya e-posta</param>
        /// <returns>Kullan�c� varsa User entity, yoksa null</returns>
        Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail);

        /// <summary>
        /// T�m aktif kullan�c�lar� getirir
        /// Admin paneli veya kullan�c� listesi i�in kullan�l�r
        /// </summary>
        /// <param name="pageNumber">Sayfa numaras� (pagination i�in)</param>
        /// <param name="pageSize">Sayfa boyutu (pagination i�in)</param>
        /// <returns>Kullan�c� listesi</returns>
        Task<List<User>> GetAllActiveUsersAsync(int pageNumber = 1, int pageSize = 10);

        /// <summary>
        /// Yeni kullan�c� olu�turur
        /// Kay�t i�lemlerinde kullan�l�r
        /// </summary>
        /// <param name="user">Olu�turulacak kullan�c� entity'si</param>
        /// <returns>Olu�turulan kullan�c� (ID ile birlikte)</returns>
        Task<User> CreateAsync(User user);

        /// <summary>
        /// Mevcut kullan�c�y� g�nceller
        /// Profil g�ncelleme, son giri� tarihi g�ncelleme vb. i�in kullan�l�r
        /// </summary>
        /// <param name="user">G�ncellenecek kullan�c� entity'si</param>
        /// <returns>G�ncellenen kullan�c�</returns>
        Task<User> UpdateAsync(User user);

        /// <summary>
        /// Kullan�c�y� siler (soft delete)
        /// G�venlik i�in genellikle kullan�c�lar tamamen silinmez, IsActive = false yap�l�r
        /// </summary>
        /// <param name="id">Silinecek kullan�c�n�n ID'si</param>
        /// <returns>��lem ba�ar�l� ise true</returns>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Kullan�c� ad�n�n benzersiz olup olmad���n� kontrol eder
        /// Kay�t i�lemlerinde kullan�l�r
        /// </summary>
        /// <param name="username">Kontrol edilecek kullan�c� ad�</param>
        /// <param name="excludeUserId">G�ncelleme i�lemlerinde mevcut kullan�c�n�n ID'si hari� tutulur</param>
        /// <returns>Kullan�c� ad� benzersiz ise true</returns>
        Task<bool> IsUsernameUniqueAsync(string username, int? excludeUserId = null);

        /// <summary>
        /// E-posta adresinin benzersiz olup olmad���n� kontrol eder
        /// Kay�t i�lemlerinde kullan�l�r
        /// </summary>
        /// <param name="email">Kontrol edilecek e-posta adresi</param>
        /// <param name="excludeUserId">G�ncelleme i�lemlerinde mevcut kullan�c�n�n ID'si hari� tutulur</param>
        /// <returns>E-posta adresi benzersiz ise true</returns>
        Task<bool> IsEmailUniqueAsync(string email, int? excludeUserId = null);

        /// <summary>
        /// Kullan�c�n�n son giri� tarihini g�nceller
        /// Her ba�ar�l� login i�leminden sonra �a�r�l�r
        /// </summary>
        /// <param name="userId">Kullan�c� ID'si</param>
        /// <returns>��lem ba�ar�l� ise true</returns>
        Task<bool> UpdateLastLoginAsync(int userId);

        /// <summary>
        /// Kullan�c�n�n e-posta do�rulama durumunu g�nceller
        /// E-posta do�rulama i�lemlerinde kullan�l�r
        /// </summary>
        /// <param name="userId">Kullan�c� ID'si</param>
        /// <param name="confirmed">Do�rulama durumu</param>
        /// <returns>��lem ba�ar�l� ise true</returns>
        Task<bool> UpdateEmailConfirmationAsync(int userId, bool confirmed);
    }
}