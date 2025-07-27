using Microsoft.EntityFrameworkCore;
using UygulamaHavuzu.Domain.Entities;

namespace UygulamaHavuzu.Persistence.Context
{
    public class ApplicationDbContext : DbContext
    {
        // Constructor - DbContext yapýlandýrma ayarlarýný alýr
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Veritabanýnda "Users" tablosunu temsil eden DbSet
        public DbSet<User> Users { get; set; }

        // Entity konfigürasyonlarýnýn yapýldýðý yer
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User entity’si için yapýlandýrma
            ConfigureUserEntity(modelBuilder);

            // Varsayýlan baþlangýç verisi ekleme
            SeedInitialData(modelBuilder);
        }

        // User entity’sine ait tablo, kolon ve index yapýlandýrmalarý
        private void ConfigureUserEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users"); // Tablo adý

                entity.HasKey(e => e.Id); // Primary Key

                entity.Property(e => e.Id)
                      .ValueGeneratedOnAdd(); // Otomatik artan Id

                entity.Property(e => e.Username)
                      .IsRequired()
                      .HasMaxLength(50)
                      .HasComment("Kullanýcý adý");

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
                      .HasComment("Hash'lenmiþ þifre");

                entity.Property(e => e.FullName)
                      .HasMaxLength(100)
                      .HasComment("Ad Soyad");

                entity.Property(e => e.IsActive)
                      .IsRequired()
                      .HasDefaultValue(true)
                      .HasComment("Kullanýcý aktif mi");

                entity.Property(e => e.EmailConfirmed)
                      .IsRequired()
                      .HasDefaultValue(false)
                      .HasComment("E-posta doðrulandý mý");

                entity.Property(e => e.CreatedAt)
                      .IsRequired()
                      .HasDefaultValueSql("GETUTCDATE()")
                      .HasComment("Oluþturulma tarihi");

                entity.Property(e => e.LastLoginAt)
                      .HasComment("Son giriþ tarihi");

                entity.Property(e => e.Role)
                      .IsRequired()
                      .HasMaxLength(20)
                      .HasDefaultValue("User")
                      .HasComment("Kullanýcý rolü");

                // Performans için oluþturulan index’ler
                entity.HasIndex(e => e.IsActive)
                      .HasDatabaseName("IX_Users_IsActive");

                entity.HasIndex(e => e.Role)
                      .HasDatabaseName("IX_Users_Role");

                entity.HasIndex(e => e.CreatedAt)
                      .HasDatabaseName("IX_Users_CreatedAt");
            });
        }

        // Baþlangýç (seed) verilerini ekler
        private void SeedInitialData(ModelBuilder modelBuilder)
        {
            // Admin kullanýcýsýnýn þifre hash’i (þimdilik placeholder)
            var adminPasswordHash = "placeholder_hash";

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    Email = "admin@uygulamahavuzu.com",
                    PasswordHash = adminPasswordHash,
                    FullName = "Sistem Yöneticisi",
                    IsActive = true,
                    EmailConfirmed = true,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    Role = "Admin"
                }
            );
        }

        // SaveChanges çaðrýldýðýnda tarihleri günceller
        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        // SaveChangesAsync çaðrýldýðýnda tarihleri günceller
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }

        // Yeni eklenen veya güncellenen entity'lerde timestamp güncellenir
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

                // Gelecekte UpdatedAt eklendiðinde burada güncellenebilir
                // if (entry.State == EntityState.Modified)
                // {
                //     entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
                // }
            }
        }
    }
}
