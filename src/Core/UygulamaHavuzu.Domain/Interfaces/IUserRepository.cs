using UygulamaHavuzu.Domain.Entities;

namespace UygulamaHavuzu.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int id);        // ID’ye göre kullanýcýyý getirir


        Task<User?> GetByUsernameAsync(string username);        // Kullanýcý adýna göre kullanýcýyý getirir (login için)


        Task<User?> GetByEmailAsync(string email);        // E-posta adresine göre kullanýcýyý getirir (login ve doðrulama için)


        Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail);        // Kullanýcý adý veya e-posta ile kullanýcýyý getirir (login için)


        Task<List<User>> GetAllActiveUsersAsync(int pageNumber = 1, int pageSize = 10);        // Sayfalama ile aktif kullanýcýlarý getirir


        Task<User> CreateAsync(User user);        // Yeni kullanýcý oluþturur (register iþlemi)


        Task<User> UpdateAsync(User user);        // Kullanýcý bilgilerini günceller (profil güncelleme vb.)


        Task<bool> DeleteAsync(int id);        // Kullanýcýyý siler (soft delete: IsActive = false yapýlýr)


        Task<bool> IsUsernameUniqueAsync(string username, int? excludeUserId = null);        // Kullanýcý adýnýn baþka kullanýcýlar tarafýndan kullanýlýp kullanýlmadýðýný kontrol eder


        Task<bool> IsEmailUniqueAsync(string email, int? excludeUserId = null);        // E-posta adresinin baþka kullanýcýlar tarafýndan kullanýlýp kullanýlmadýðýný kontrol eder


        Task<bool> UpdateLastLoginAsync(int userId);        // Kullanýcýnýn son giriþ tarihini günceller (login sonrasý çaðrýlýr)


        Task<bool> UpdateEmailConfirmationAsync(int userId, bool confirmed);         // Kullanýcýnýn e-posta doðrulama durumunu günceller



    }
}

// Bu interface, kullanýcýya (User entity) ait veri eriþim iþlemlerini tanýmlar. 
// Clean Architecture prensiplerine göre hazýrlanmýþ olup, Domain katmanýnda yer alýr 
// ve veri eriþim detaylarýndan baðýmsýzdýr. IUserRepository, uygulama boyunca kullanýcý 
// oluþturma, güncelleme, silme, arama gibi iþlemlerde kullanýlmak üzere 
// Persistence katmanýnda implemente edilir. Bu sayede loosely coupled (gevþek baðlý) 
// ve test edilebilir bir yapý saðlanýr.
