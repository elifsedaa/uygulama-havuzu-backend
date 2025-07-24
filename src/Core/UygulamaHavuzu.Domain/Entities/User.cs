using System.ComponentModel.DataAnnotations;

namespace UygulamaHavuzu.Domain.Entities
{
    public class User
    {
        public int ID { get; set; }
        // Benzersiz kullanici kimligi (primary key)

        // Validation 
        [Required(ErrorMessage = "Kullanici Adi Zorunludur")]
        [MaxLength(50, ErrorMessage = "Kullanici adi 50 Karakterden Fazla Olamaz!")]
        public string Username { get; set; } = string.Empty;
        // giriste kullanilan kullanici adi : zorunlu ve max 50 karakter

        [Required(ErrorMessage = "E-posta Adresi Zorunludur")]
        [EmailAddress(ErrorMessage = "Gecerli bir e-post adresi giriniz!")]
        [MaxLength(100, ErrorMessage = "e-posta adresi 100 Karakterden Fazla Olamaz!")]
        // E-posta adresi - Zorunlu, format kontrolü var, en fazla 100 karakter
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Sifre Zorunludur!")]
        public string PasswordHash { get; set; } = string.Empty;
        //Sifre hash degeri ( sifre duz metin olarak tutulmuyor )


        [MaxLength(100, ErrorMessage = "Ad Soyad 100 Karakterden Fazla Olamaz!")]
        public string? FullName { get; set; }

        // Kullanıcı aktif mi? false ise giriş yapamaz
        public bool IsActive { get; set; } = true;

        // E-posta doğrulandı mı? false ise güvenlik önlemi alınabilir
        public bool EmailConfirmed { get; set; } = false;

        // Hesabın oluşturulduğu tarih (varsayılan olarak şu anki UTC zamanı)
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Son giriş zamanı - Boş olabilir (null), hiç giriş yapmamış kullanıcılar için
        public DateTime? LastLoginAt { get; set; }

        // Kullanıcının yetkisi/rolü (örneğin: "User", "Admin") - En fazla 20 karakter
        [MaxLength(20, ErrorMessage = "Rol 20 karakterden fazla olamaz")]
        public string Role { get; set; } = "User";
    }
}