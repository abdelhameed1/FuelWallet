using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FuelWallet.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedDefaultUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                SET IDENTITY_INSERT [Users] ON;
                IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [Id] = 1)
                    INSERT INTO [Users] ([Id],[CreatedAt],[PasswordHash],[UpdatedAt],[Username])
                    VALUES (1, '2026-01-01T00:00:00Z', '$2a$11$lzaqMxWYuz.RUodv.jkjVeOItX/ZbbLbgafajVghbfzEt1BvRAZh6', NULL, 'station-api');
                SET IDENTITY_INSERT [Users] OFF;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
