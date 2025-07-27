using UygulamaHavuzu.Domain.Entities;

namespace UygulamaHavuzu.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int id);        // ID�ye g�re kullan�c�y� getirir


        Task<User?> GetByUsernameAsync(string username);        // Kullan�c� ad�na g�re kullan�c�y� getirir (login i�in)


        Task<User?> GetByEmailAsync(string email);        // E-posta adresine g�re kullan�c�y� getirir (login ve do�rulama i�in)


        Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail);        // Kullan�c� ad� veya e-posta ile kullan�c�y� getirir (login i�in)


        Task<List<User>> GetAllActiveUsersAsync(int pageNumber = 1, int pageSize = 10);        // Sayfalama ile aktif kullan�c�lar� getirir


        Task<User> CreateAsync(User user);        // Yeni kullan�c� olu�turur (register i�lemi)


        Task<User> UpdateAsync(User user);        // Kullan�c� bilgilerini g�nceller (profil g�ncelleme vb.)


        Task<bool> DeleteAsync(int id);        // Kullan�c�y� siler (soft delete: IsActive = false yap�l�r)


        Task<bool> IsUsernameUniqueAsync(string username, int? excludeUserId = null);        // Kullan�c� ad�n�n ba�ka kullan�c�lar taraf�ndan kullan�l�p kullan�lmad���n� kontrol eder


        Task<bool> IsEmailUniqueAsync(string email, int? excludeUserId = null);        // E-posta adresinin ba�ka kullan�c�lar taraf�ndan kullan�l�p kullan�lmad���n� kontrol eder


        Task<bool> UpdateLastLoginAsync(int userId);        // Kullan�c�n�n son giri� tarihini g�nceller (login sonras� �a�r�l�r)


        Task<bool> UpdateEmailConfirmationAsync(int userId, bool confirmed);         // Kullan�c�n�n e-posta do�rulama durumunu g�nceller



    }
}

// Bu interface, kullan�c�ya (User entity) ait veri eri�im i�lemlerini tan�mlar. 
// Clean Architecture prensiplerine g�re haz�rlanm�� olup, Domain katman�nda yer al�r 
// ve veri eri�im detaylar�ndan ba��ms�zd�r. IUserRepository, uygulama boyunca kullan�c� 
// olu�turma, g�ncelleme, silme, arama gibi i�lemlerde kullan�lmak �zere 
// Persistence katman�nda implemente edilir. Bu sayede loosely coupled (gev�ek ba�l�) 
// ve test edilebilir bir yap� sa�lan�r.
