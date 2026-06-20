using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FuelWallet.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedWallets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FuelTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WalletId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StationId = table.Column<int>(type: "int", nullable: false),
                    PumpId = table.Column<int>(type: "int", nullable: false),
                    RequestedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AuthorizedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RejectionReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RequestReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FuelTransactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Wallets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WalletId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CustomerId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    VehiclePlate = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DailyLimit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wallets", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Wallets",
                columns: new[] { "Id", "Balance", "CreatedAt", "CustomerId", "CustomerName", "DailyLimit", "IsActive", "UpdatedAt", "VehiclePlate", "WalletId" },
                values: new object[,]
                {
                    { 1, 500.00m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "CUST-001", "Ahmed Hassan", 300.00m, true, null, "ABC-1234", "WLT-1001" },
                    { 2, 50.00m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "CUST-002", "Sara Mostafa", 200.00m, true, null, "XYZ-5678", "WLT-1002" },
                    { 3, 1000.00m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "CUST-003", "Omar Khalil", 100.00m, true, null, "DEF-9999", "WLT-1003" },
                    { 4, 500.00m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "CUST-004", "Layla Ibrahim", 300.00m, false, null, "GHI-4321", "WLT-1004" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_FuelTransactions_RequestReference",
                table: "FuelTransactions",
                column: "RequestReference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_WalletId",
                table: "Wallets",
                column: "WalletId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FuelTransactions");

            migrationBuilder.DropTable(
                name: "Wallets");
        }
    }
}
