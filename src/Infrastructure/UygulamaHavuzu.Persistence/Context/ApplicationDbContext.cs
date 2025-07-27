using Microsoft.EntityFrameworkCore;
using UygulamaHavuzu.Domain.Entities;

namespace UygulamaHavuzu.Persistence.Context
{
    public class ApplicationDbContext : DbContext
    {
        // Constructor - DbContext yap�land�rma ayarlar�n� al�r
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Veritaban�nda "Users" tablosunu temsil eden DbSet
        public DbSet<User> Users { get; set; }

        // Entity konfig�rasyonlar�n�n yap�ld��� yer
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User entity�si i�in yap�land�rma
            ConfigureUserEntity(modelBuilder);

            // Varsay�lan ba�lang�� verisi ekleme
            SeedInitialData(modelBuilder);
        }

        // User entity�sine ait tablo, kolon ve index yap�land�rmalar�
        private void ConfigureUserEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users"); // Tablo ad�

                entity.HasKey(e => e.Id); // Primary Key

                entity.Property(e => e.Id)
                      .ValueGeneratedOnAdd(); // Otomatik artan Id

                entity.Property(e => e.Username)
                      .IsRequired()
                      .HasMaxLength(50)
                      .HasComment("Kullan�c� ad�");

                entity.HasIndex(e => e.Username)
                      .IsUnique()
                      .HasDatabaseName("IX_Users_Username"); // Unique index

                entity.Property(e => e.Email)
                      .IsRequired()
                      .HasMaxLength(100)
                      .HasComment("E-posta adresi");

                entity.HasIndex(e => e.Email)
                      .IsUnique()
                      .HasDatabaseName("IX_Users_Email");

                entity.Property(e => e.PasswordHash)
                      .IsRequired()
                      .HasMaxLength(500)
                      .HasComment("Hash'lenmi� �ifre");

                entity.Property(e => e.FullName)
                      .HasMaxLength(100)
                      .HasComment("Ad Soyad");

                entity.Property(e => e.IsActive)
                      .IsRequired()
                      .HasDefaultValue(true)
                      .HasComment("Kullan�c� aktif mi");

                entity.Property(e => e.EmailConfirmed)
                      .IsRequired()
                      .HasDefaultValue(false)
                      .HasComment("E-posta do�ruland� m�");

                entity.Property(e => e.CreatedAt)
                      .IsRequired()
                      .HasDefaultValueSql("GETUTCDATE()")
                      .HasComment("Olu�turulma tarihi");

                entity.Property(e => e.LastLoginAt)
                      .HasComment("Son giri� tarihi");

                entity.Property(e => e.Role)
                      .IsRequired()
                      .HasMaxLength(20)
                      .HasDefaultValue("User")
                      .HasComment("Kullan�c� rol�");

                // Performans i�in olu�turulan index�ler
                entity.HasIndex(e => e.IsActive)
                      .HasDatabaseName("IX_Users_IsActive");

                entity.HasIndex(e => e.Role)
                      .HasDatabaseName("IX_Users_Role");

                entity.HasIndex(e => e.CreatedAt)
                      .HasDatabaseName("IX_Users_CreatedAt");
            });
        }

        // Ba�lang�� (seed) verilerini ekler
        private void SeedInitialData(ModelBuilder modelBuilder)
        {
            // Admin kullan�c�s�n�n �ifre hash�i (�imdilik placeholder)
            var adminPasswordHash = "placeholder_hash";

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    Email = "admin@uygulamahavuzu.com",
                    PasswordHash = adminPasswordHash,
                    FullName = "Sistem Y�neticisi",
                    IsActive = true,
                    EmailConfirmed = true,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    Role = "Admin"
                }
            );
        }

        // SaveChanges �a�r�ld���nda tarihleri g�nceller
        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        // SaveChangesAsync �a�r�ld���nda tarihleri g�nceller
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }

        // Yeni eklenen veya g�ncellenen entity'lerde timestamp g�ncellenir
        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    if (entry.Property("CreatedAt").CurrentValue == null ||
                        (DateTime)entry.Property("CreatedAt").CurrentValue == default)
                    {
                        entry.Property("CreatedAt").CurrentValue = DateTime.UtcNow;
                    }
                }

                // Gelecekte UpdatedAt eklendi�inde burada g�ncellenebilir
                // if (entry.State == EntityState.Modified)
                // {
                //     entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
                // }
            }
        }
    }
}
