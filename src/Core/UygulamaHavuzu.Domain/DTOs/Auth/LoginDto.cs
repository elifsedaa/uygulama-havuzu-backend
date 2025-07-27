using System.ComponentModel.DataAnnotations;

namespace UygulamaHavuzu.Domain.DTOs.Auth
{
    /// <summary>
    /// Kullan�c�n�n giri� i�lemi s�ras�nda frontend'den backend'e g�nderdi�i verileri temsil eder.
    /// Genellikle Login API iste�inde request body olarak kullan�l�r.
    /// </summary>
    /// h�ug�u
    public class LoginDto
    {
        [Required(ErrorMessage = " Kullanici adi ve e-posta zorunludur")] // asp.net model validation s�ras�nda kontrol edilir.
        public string UsernameOrEmail { get; set; } = string.Empty; // hem kullanici adi hem de e-posta ile giris imkani

        [Required(ErrorMessage = "Sifre Zorunludur")]
        [MinLength(8, ErrorMessage = " Sifre en az 8 karakter olmalidir")]
        public string Password { get; set; } = string.Empty;
        // �ifre d�z metin olarak gelicek hash'leme i�lemi backend taraf�ndan yap�l�r.

        public bool RememberMe { get; set; } = false;
        // kullan�c� 'beni hatirla' secerse token suresi uzatilacak
    }


    /// <summary>
    /// Kullan�c� kayd� s�ras�nda frontend'den backend'e g�nderilen verileri ta��r.
    /// Register API'sinde request body olarak kullan�l�r.
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
        //RegularExpression bit metnin istenen formatta olmas�n� kontrol eden validasyon 
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]",
            ErrorMessage = "�ifre en az bir k���k harf, bir b�y�k harf, bir rakam ve bir �zel karakter i�ermelidir")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "�ifre do�rulama zorunludur")]
        [Compare("Password", ErrorMessage = "�ifreler e�le�miyor")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [MaxLength(100, ErrorMessage = "Ad Soyad 100 karakterden fazla olamaz")]
        public string? FullName { get; set; }
    }

    public class LoginResponseDto // girisin basarili oldugunda frontende donen yan�t yapisi, JWT token, token suresi, kullanici bilgilerini icerir
    {
        public string Token { get; set; } = string.Empty;
        // kimlik dogrulama icin kullanilacak JWT token

        public DateTime ExpiresAt { get; set; } //token in gecerli oldugu son tarih zamani tutar
                                                //
        public UserInfoDto User { get; set; } = new(); // kullaniciya ait temel bilgiler
    }


    public class UserInfoDto
    {
        public int Id { get; set; } // Kullan�c�n�n veritaban�ndaki ID'si
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; } // �ste�e ba�l� tam ad�
        public string Role { get; set; } = string.Empty; // Kullan�c�n�n rol� (Admin, User)
        public bool EmailConfirmed { get; set; } // E-posta adresi onaylanm�� m�
        public DateTime CreatedAt { get; set; } // Kullan�c�n�n olu�turulma zaman�
        public DateTime? LastLoginAt { get; set; } // Son giri� tarihi (hi� giri� yap�lmad�ysa null olabilir)
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string>? Errors { get; set; }

        public static ApiResponse<T> SuccessResult(T data, string message = "��lem ba�ar�l�")
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
LoginDto ve RegisterDto istek (request) s�ras�nda frontend�den backend�e gelen verileri temsil eder. Burada do�rulama ve sadece ihtiya� duyulan bilgiler var.

LoginResponseDto ise cevap (response) olarak backend�den frontend�e d�nen verileri kapsar. Mesela JWT token ve kullan�c� bilgileri.

UserInfoDto sadece frontend�in g�rmesi gereken kullan�c� bilgilerini ta��r, hassas veriler yok.

ApiResponse<T> ise API'nin standart bir cevap yap�s�d�r, hem veri hem durum bilgisini d�zenler.
  */