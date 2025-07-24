using UygulamaHavuzu.Domain.Entities;

namespace UygulamaHavuzu.Domain.Interfaces
{
    /// <summary>
    /// Kullanýcý veri eriþim iþlemleri için interface
    /// Clean Architecture'da Domain katmaný Infrastructure'dan baðýmsýz olmalý
    /// Bu interface Persistence katmanýnda implement edilecek
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Kullanýcý ID'sine göre kullanýcý getirir
        /// </summary>
        /// <param name="id">Kullanýcý ID'si</param>
        /// <returns>Kullanýcý varsa User entity, yoksa null</returns>
        Task<User?> GetByIdAsync(int id);

        /// <summary>
        /// Kullanýcý adýna göre kullanýcý getirir
        /// Login iþlemlerinde kullanýlýr
        /// </summary>
        /// <param name="username">Kullanýcý adý</param>
        /// <returns>Kullanýcý varsa User entity, yoksa null</returns>
        Task<User?> GetByUsernameAsync(string username);

        /// <summary>
        /// E-posta adresine göre kullanýcý getirir
        /// Login ve e-posta doðrulama iþlemlerinde kullanýlýr
        /// </summary>
        /// <param name="email">E-posta adresi</param>
        /// <returns>Kullanýcý varsa User entity, yoksa null</returns>
        Task<User?> GetByEmailAsync(string email);

        /// <summary>
        /// Kullanýcý adý veya e-posta ile kullanýcý getirir
        /// Login iþlemlerinde kullanýcý hem username hem email ile giriþ yapabilir
        /// </summary>
        /// <param name="usernameOrEmail">Kullanýcý adý veya e-posta</param>
        /// <returns>Kullanýcý varsa User entity, yoksa null</returns>
        Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail);

        /// <summary>
        /// Tüm aktif kullanýcýlarý getirir
        /// Admin paneli veya kullanýcý listesi için kullanýlýr
        /// </summary>
        /// <param name="pageNumber">Sayfa numarasý (pagination için)</param>
        /// <param name="pageSize">Sayfa boyutu (pagination için)</param>
        /// <returns>Kullanýcý listesi</returns>
        Task<List<User>> GetAllActiveUsersAsync(int pageNumber = 1, int pageSize = 10);

        /// <summary>
        /// Yeni kullanýcý oluþturur
        /// Kayýt iþlemlerinde kullanýlýr
        /// </summary>
        /// <param name="user">Oluþturulacak kullanýcý entity'si</param>
        /// <returns>Oluþturulan kullanýcý (ID ile birlikte)</returns>
        Task<User> CreateAsync(User user);

        /// <summary>
        /// Mevcut kullanýcýyý günceller
        /// Profil güncelleme, son giriþ tarihi güncelleme vb. için kullanýlýr
        /// </summary>
        /// <param name="user">Güncellenecek kullanýcý entity'si</param>
        /// <returns>Güncellenen kullanýcý</returns>
        Task<User> UpdateAsync(User user);

        /// <summary>
        /// Kullanýcýyý siler (soft delete)
        /// Güvenlik için genellikle kullanýcýlar tamamen silinmez, IsActive = false yapýlýr
        /// </summary>
        /// <param name="id">Silinecek kullanýcýnýn ID'si</param>
        /// <returns>Ýþlem baþarýlý ise true</returns>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Kullanýcý adýnýn benzersiz olup olmadýðýný kontrol eder
        /// Kayýt iþlemlerinde kullanýlýr
        /// </summary>
        /// <param name="username">Kontrol edilecek kullanýcý adý</param>
        /// <param name="excludeUserId">Güncelleme iþlemlerinde mevcut kullanýcýnýn ID'si hariç tutulur</param>
        /// <returns>Kullanýcý adý benzersiz ise true</returns>
        Task<bool> IsUsernameUniqueAsync(string username, int? excludeUserId = null);

        /// <summary>
        /// E-posta adresinin benzersiz olup olmadýðýný kontrol eder
        /// Kayýt iþlemlerinde kullanýlýr
        /// </summary>
        /// <param name="email">Kontrol edilecek e-posta adresi</param>
        /// <param name="excludeUserId">Güncelleme iþlemlerinde mevcut kullanýcýnýn ID'si hariç tutulur</param>
        /// <returns>E-posta adresi benzersiz ise true</returns>
        Task<bool> IsEmailUniqueAsync(string email, int? excludeUserId = null);

        /// <summary>
        /// Kullanýcýnýn son giriþ tarihini günceller
        /// Her baþarýlý login iþleminden sonra çaðrýlýr
        /// </summary>
        /// <param name="userId">Kullanýcý ID'si</param>
        /// <returns>Ýþlem baþarýlý ise true</returns>
        Task<bool> UpdateLastLoginAsync(int userId);

        /// <summary>
        /// Kullanýcýnýn e-posta doðrulama durumunu günceller
        /// E-posta doðrulama iþlemlerinde kullanýlýr
        /// </summary>
        /// <param name="userId">Kullanýcý ID'si</param>
        /// <param name="confirmed">Doðrulama durumu</param>
        /// <returns>Ýþlem baþarýlý ise true</returns>
        Task<bool> UpdateEmailConfirmationAsync(int userId, bool confirmed);
    }
}