using System.ComponentModel.DataAnnotations;

namespace UygulamaHavuzu.Domain.DTOs.Auth
{
    /// <summary>
    /// Kullanýcýnýn giriþ iþlemi sýrasýnda frontend'den backend'e gönderdiði verileri temsil eder.
    /// Genellikle Login API isteðinde request body olarak kullanýlýr.
    /// </summary>
    /// 
    public class LoginDto
    {
        [Required(ErrorMessage = " Kullanici adi ve e-posta zorunludur")] // asp.net model validation sýrasýnda kontrol edilir.
        public string UsernameOrEmail { get; set; } = string.Empty; // hem kullanici adi hem de e-posta ile giris imkani

        [Required(ErrorMessage = "Sifre Zorunludur")]
        [MinLength(8, ErrorMessage = " Sifre en az 8 karakter olmalidir")]
        public string Password { get; set; } = string.Empty;
        // þifre düz metin olarak gelicek hash'leme iþlemi backend tarafýndan yapýlýr.

        public bool RememberMe { get; set; } = false;
        // kullanýcý 'beni hatirla' secerse token suresi uzatilacak
    }


    /// <summary>
    /// Kullanýcý kaydý sýrasýnda frontend'den backend'e gönderilen verileri taþýr.
    /// Register API'sinde request body olarak kullanýlýr.
    /// </summary>

    public class RegisterDto //kayit formundaki alanlari temsil edicek
    {
        [Required(ErrorMessage = "Kullanici adi zorunludur")]
        [MinLength(3, ErrorMessage = "Kullanici adi en az 8 karakter olmalidir")]
        [MaxLength(50, ErrorMessage = "Kullanici adi 50 karakterden fazla olmamali")]
        public string Username { get; set; } = string.Empty;
    
        [Required(ErrorMessage = "e-posta adresi  zorunludur")]
        [EmailAddress(ErrorMessage = "Gecerli bir eposta adresi giriniz")]
        [MaxLength(100, ErrorMessage = "Eposta adresi 50 karakterden fazla olmamali")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Sifre Zorunludur")]
        [MinLength(8, ErrorMessage = "Sifre en az 3 karakter olmalidir")]
        //RegularExpression bit metnin istenen formatta olmasýný kontrol eden validasyon 
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]",
            ErrorMessage = "Þifre en az bir küçük harf, bir büyük harf, bir rakam ve bir özel karakter içermelidir")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Þifre doðrulama zorunludur")]
        [Compare("Password", ErrorMessage = "Þifreler eþleþmiyor")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [MaxLength(100, ErrorMessage = "Ad Soyad 100 karakterden fazla olamaz")]
        public string? FullName { get; set; }
    }

    public class LoginResponseDto // girisin basarili oldugunda frontende donen yanýt yapisi, JWT token, token suresi, kullanici bilgilerini icerir
    {
        public string Token { get; set; } = string.Empty;
        // kimlik dogrulama icin kullanilacak JWT token

        public DateTime ExpiresAt { get; set; } //token in gecerli oldugu son tarih zamani tutar
                                                //
        public UserInfoDto User { get; set; } = new(); // kullaniciya ait temel bilgiler
    }


    public class UserInfoDto
    {
        public int Id { get; set; } // Kullanýcýnýn veritabanýndaki ID'si
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; } // Ýsteðe baðlý tam adý
        public string Role { get; set; } = string.Empty; // Kullanýcýnýn rolü (Admin, User)
        public bool EmailConfirmed { get; set; } // E-posta adresi onaylanmýþ mý
        public DateTime CreatedAt { get; set; } // Kullanýcýnýn oluþturulma zamaný
        public DateTime? LastLoginAt { get; set; } // Son giriþ tarihi (hiç giriþ yapýlmadýysa null olabilir)
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string>? Errors { get; set; }

        public static ApiResponse<T> SuccessResult(T data, string message = "Ýþlem baþarýlý")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static ApiResponse<T> ErrorResult(string message, List<string>? errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Errors = errors
            };
        }
    }
}


 /*
LoginDto ve RegisterDto istek (request) sýrasýnda frontend’den backend’e gelen verileri temsil eder. Burada doðrulama ve sadece ihtiyaç duyulan bilgiler var.

LoginResponseDto ise cevap (response) olarak backend’den frontend’e dönen verileri kapsar. Mesela JWT token ve kullanýcý bilgileri.

UserInfoDto sadece frontend’in görmesi gereken kullanýcý bilgilerini taþýr, hassas veriler yok.

ApiResponse<T> ise API'nin standart bir cevap yapýsýdýr, hem veri hem durum bilgisini düzenler.
  */