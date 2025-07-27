using Microsoft.EntityFrameworkCore;
using UygulamaHavuzu.Domain.Interfaces;
using UygulamaHavuzu.Persistence.Context;
using DomainUser = UygulamaHavuzu.Domain.Entities.User;

namespace UygulamaHavuzu.Persistence.Repositories
{
    // DomainUser entity'si için repository implementation sınıfı
    // Domain katmanındaki IUserRepository interface'ini implement eder
    // Entity Framework kullanarak veritabanı işlemlerini gerçekleştirir
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        // Constructor - Dependency Injection ile DbContext alır
        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Kullanıcı ID'sine göre kullanıcı getirir
        // AsNoTracking() performans için kullanılır - sadece okuma işlemi
        public async Task<DomainUser?> GetByIdAsync(int id)
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        // Kullanıcı adına göre kullanıcı getirir
        // Login işlemlerinde kullanılır
        public async Task<DomainUser?> GetByUsernameAsync(string username)
        {
            if (string.IsNullOrEmpty(username))
                return null;

            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
        }

        // E-posta adresine göre kullanıcı getirir
        // Login ve e-posta doğrulama işlemlerinde kullanılır
        public async Task<DomainUser?> GetByEmailAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
                return null;

            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        // Kullanıcı adı veya e-posta ile kullanıcı getirir
        // Login işlemlerinde kullanıcı hem username hem email ile giriş yapabilir
        public async Task<DomainUser?> GetByUsernameOrEmailAsync(string usernameOrEmail)
        {
            if (string.IsNullOrEmpty(usernameOrEmail))
                return null;

            var lowerInput = usernameOrEmail.ToLower();

            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u =>
                    u.Username.ToLower() == lowerInput ||
                    u.Email.ToLower() == lowerInput);
        }

        // Tüm aktif kullanıcıları sayfalama ile getirir
        // Admin paneli veya kullanıcı listesi için kullanılır
        public async Task<List<DomainUser>> GetAllActiveUsersAsync(int pageNumber = 1, int pageSize = 10)
        {
            // Sayfa numarası en az 1 olmalı
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100; // DoS attack prevention

            var skip = (pageNumber - 1) * pageSize;

            return await _context.Users
                .AsNoTracking()
                .Where(u => u.IsActive)
                .OrderBy(u => u.CreatedAt) // En yeni kullanıcılar en başta
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();
        }

        // Yeni kullanıcı oluşturur
        // Kayıt işlemlerinde kullanılır
        public async Task<DomainUser> CreateAsync(DomainUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            // CreatedAt otomatik olarak ayarlanır (DbContext'te)
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user; // Id değeri otomatik olarak ayarlanmış olur
        }

        // Mevcut kullanıcıyı günceller
        // Profil güncelleme, son giriş tarihi güncelleme vb. için kullanılır
        public async Task<DomainUser> UpdateAsync(DomainUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return user;
        }

        // Kullanıcıyı siler (soft delete)
        // Güvenlik için genellikle kullanıcılar tamamen silinmez, IsActive = false yapılır
        public async Task<bool> DeleteAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return false;

            // Soft delete - kullanıcıyı tamamen silmek yerine deaktive et
            user.IsActive = false;
            _context.Users.Update(user);

            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        // Kullanıcı adının benzersiz olup olmadığını kontrol eder
        // Kayıt işlemlerinde kullanılır
        public async Task<bool> IsUsernameUniqueAsync(string username, int? excludeUserId = null)
        {
            if (string.IsNullOrEmpty(username))
                return false;

            var query = _context.Users
                .AsNoTracking()
                .Where(u => u.Username.ToLower() == username.ToLower());

            // Güncelleme işlemlerinde mevcut kullanıcının ID'si hariç tutulur
            if (excludeUserId.HasValue)
            {
                query = query.Where(u => u.Id != excludeUserId.Value);
            }

            var exists = await query.AnyAsync();
            return !exists; // Kullanıcı yoksa benzersizdir (true döner)
        }

        // E-posta adresinin benzersiz olup olmadığını kontrol eder
        // Kayıt işlemlerinde kullanılır
        public async Task<bool> IsEmailUniqueAsync(string email, int? excludeUserId = null)
        {
            if (string.IsNullOrEmpty(email))
                return false;

            var query = _context.Users
                .AsNoTracking()
                .Where(u => u.Email.ToLower() == email.ToLower());

            // Güncelleme işlemlerinde mevcut kullanıcının ID'si hariç tutulur
            if (excludeUserId.HasValue)
            {
                query = query.Where(u => u.Id != excludeUserId.Value);
            }

            var exists = await query.AnyAsync();
            return !exists; // E-posta yoksa benzersizdir (true döner)
        }

        // Kullanıcının son giriş tarihini günceller
        // Her başarılı login işleminden sonra çağrılır
        // Performans için sadece bu alanı günceller
        public async Task<bool> UpdateLastLoginAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;

            user.LastLoginAt = DateTime.UtcNow;

            // Sadece LastLoginAt alanını güncelle (performans için)
            _context.Entry(user).Property(u => u.LastLoginAt).IsModified = true;

            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        // Kullanıcının e-posta doğrulama durumunu günceller
        // E-posta doğrulama işlemlerinde kullanılır
        public async Task<bool> UpdateEmailConfirmationAsync(int userId, bool confirmed)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;

            user.EmailConfirmed = confirmed;

            // Sadece EmailConfirmed alanını güncelle (performans için)
            _context.Entry(user).Property(u => u.EmailConfirmed).IsModified = true;

            var result = await _context.SaveChangesAsync();
            return result > 0;
        }
    }
}