using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using UygulamaHavuzu.Application.Interfaces;
using UygulamaHavuzu.Application.Services;
using UygulamaHavuzu.Domain.Interfaces;
using UygulamaHavuzu.Persistence.Context;
using UygulamaHavuzu.Persistence.Repositories;
using Npgsql.EntityFrameworkCore.PostgreSQL;


var builder = WebApplication.CreateBuilder(args);

// Configuration'� al
var configuration = builder.Configuration;

// Entity Framework ve SQL Server ba�lant�s�
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

// Repository'leri DI container'a ekle
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Application Service'leri DI container'a ekle
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();

// JWT Authentication konfig�rasyonu
var jwtSecretKey = configuration["JWT:SecretKey"];
var jwtIssuer = configuration["JWT:Issuer"];
var jwtAudience = configuration["JWT:Audience"];

builder.Services.AddAuthentication(options =>
{
    // Varsay�lan authentication scheme'i JWT olarak ayarla
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        // �mza do�rulamas� yap
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),

        // Issuer do�rulamas� yap
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,

        // Audience do�rulamas� yap
        ValidateAudience = true,
        ValidAudience = jwtAudience,

        // Token s�resini do�rula
        ValidateLifetime = true,

        // Saat fark� tolerans� (5 dakika)
        ClockSkew = TimeSpan.FromMinutes(5)
    };

    // JWT token'� Authorization header'dan al
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // Token'� Authorization header'dan oku
            var token = context.Request.Headers["Authorization"]
                .FirstOrDefault()?.Split(" ").Last();

            if (!string.IsNullOrEmpty(token))
            {
                context.Token = token;
            }

            return Task.CompletedTask;
        },

        // Authentication ba�ar�s�z oldu�unda
        OnAuthenticationFailed = context =>
        {
            // Log authentication failures (implement logging later)
            Console.WriteLine($"Authentication failed: {context.Exception.Message}");
            return Task.CompletedTask;
        },

        // Token do�ruland�ktan sonra
        OnTokenValidated = context =>
        {
            // Additional validation logic burada yap�labilir
            return Task.CompletedTask;
        }
    };
});

// Authorization policy'leri
builder.Services.AddAuthorization(options =>
{
    // Admin rol� i�in policy
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));

    // Normal kullan�c� policy'si
    options.AddPolicy("UserOnly", policy =>
        policy.RequireRole("User", "Admin"));
});

// CORS konfig�rasyonu
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        var allowedOrigins = configuration.GetSection("AllowedOrigins").Get<string[]>() ??
                           new[] { "http://localhost:4200" };

        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Cookie ve Authorization header i�in
    });
});

// Controller'lar� ekle
builder.Services.AddControllers();

// API Explorer (Swagger i�in)
builder.Services.AddEndpointsApiExplorer();

// Swagger konfig�rasyonu - JWT token deste�i ile
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Uygulama Havuzu API",
        Version = "v1",
        Description = "Uygulama Havuzu Backend API"
    });

    // JWT Bearer token i�in Swagger UI'da Authorization butonu
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// Development ortam�nda Swagger kullan
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Uygulama Havuzu API V1");
        c.RoutePrefix = string.Empty; // Swagger UI'� root'ta a�
    });
}

// HTTPS y�nlendirme
app.UseHttpsRedirection();

// CORS middleware'i - Authentication'dan �nce olmal�
app.UseCors("AllowFrontend");

// Authentication middleware'i
app.UseAuthentication();

// Authorization middleware'i
app.UseAuthorization();

// Controller routing
app.MapControllers();

// Veritaban�n� otomatik migrate et (development i�in)
// Production'da migration'lar manuel olarak �al��t�r�lmal�
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        try
        {
            // Veritaban�n� olu�tur ve migration'lar� uygula
            context.Database.Migrate();
            Console.WriteLine("Database migration completed successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Database migration failed: {ex.Message}");
        }
    }
}

app.Run();