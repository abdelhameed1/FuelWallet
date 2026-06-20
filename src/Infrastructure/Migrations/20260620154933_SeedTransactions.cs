using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FuelWallet.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "FuelTransactions",
                columns: new[] { "Id", "AuthorizedAmount", "CreatedAt", "PumpId", "RejectionReason", "RequestReference", "RequestedAmount", "StationId", "Status", "UpdatedAt", "WalletId" },
                values: new object[,]
                {
                    { 1, 100m, new DateTime(2026, 6, 18, 8, 0, 0, 0, DateTimeKind.Utc), 1, null, "SEED-REF-001", 100m, 101, "Authorized", new DateTime(2026, 6, 18, 8, 0, 1, 0, DateTimeKind.Utc), "WLT-1001" },
                    { 2, 200m, new DateTime(2026, 6, 17, 10, 0, 0, 0, DateTimeKind.Utc), 2, null, "SEED-REF-002", 200m, 102, "Authorized", new DateTime(2026, 6, 17, 10, 0, 1, 0, DateTimeKind.Utc), "WLT-1001" },
                    { 3, null, new DateTime(2026, 6, 19, 9, 0, 0, 0, DateTimeKind.Utc), 3, "Wallet balance is insufficient for this transaction.", "SEED-REF-003", 300m, 101, "Rejected", new DateTime(2026, 6, 19, 9, 0, 0, 0, DateTimeKind.Utc), "WLT-1002" },
                    { 4, 80m, new DateTime(2026, 6, 16, 14, 0, 0, 0, DateTimeKind.Utc), 1, null, "SEED-REF-004", 80m, 103, "Authorized", new DateTime(2026, 6, 16, 14, 0, 1, 0, DateTimeKind.Utc), "WLT-1003" },
                    { 5, null, new DateTime(2026, 6, 15, 11, 0, 0, 0, DateTimeKind.Utc), 2, null, "SEED-REF-005", 50m, 101, "Expired", new DateTime(2026, 6, 15, 11, 2, 0, 0, DateTimeKind.Utc), "WLT-1003" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "FuelTransactions",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "FuelTransactions",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "FuelTransactions",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "FuelTransactions",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "FuelTransactions",
                keyColumn: "Id",
                keyValue: 5);
        }
    }
}
