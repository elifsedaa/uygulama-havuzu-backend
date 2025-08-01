﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UygulamaHavuzu.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "Kullanıcı adı"),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "E-posta adresi"),
                    PasswordHash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false, comment: "Hash'lenmiş şifre"),
                    FullName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "Ad Soyad"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true, comment: "Kullanıcı aktif mi"),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false, comment: "E-posta doğrulandı mı"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()", comment: "Oluşturulma tarihi"),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Son giriş tarihi"),
                    Role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "User", comment: "Kullanıcı rolü")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "EmailConfirmed", "FullName", "IsActive", "LastLoginAt", "PasswordHash", "Role", "Username" },
                values: new object[] { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin@uygulamahavuzu.com", true, "Sistem Yöneticisi", true, null, "placeholder_hash", "Admin", "admin" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_CreatedAt",
                table: "Users",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_IsActive",
                table: "Users",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Role",
                table: "Users",
                column: "Role");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
