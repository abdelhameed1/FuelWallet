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
            migrationBuilder.Sql(@"
                SET IDENTITY_INSERT [FuelTransactions] ON;
                IF NOT EXISTS (SELECT 1 FROM [FuelTransactions] WHERE [Id] = 1)
                    INSERT INTO [FuelTransactions] ([Id],[AuthorizedAmount],[CreatedAt],[PumpId],[RejectionReason],[RequestReference],[RequestedAmount],[StationId],[Status],[UpdatedAt],[WalletId])
                    VALUES (1, 100.00, '2026-06-18T08:00:00Z', 1, NULL, 'SEED-REF-001', 100.00, 101, 'Authorized', '2026-06-18T08:00:01Z', 'WLT-1001');
                IF NOT EXISTS (SELECT 1 FROM [FuelTransactions] WHERE [Id] = 2)
                    INSERT INTO [FuelTransactions] ([Id],[AuthorizedAmount],[CreatedAt],[PumpId],[RejectionReason],[RequestReference],[RequestedAmount],[StationId],[Status],[UpdatedAt],[WalletId])
                    VALUES (2, 200.00, '2026-06-17T10:00:00Z', 2, NULL, 'SEED-REF-002', 200.00, 102, 'Authorized', '2026-06-17T10:00:01Z', 'WLT-1001');
                IF NOT EXISTS (SELECT 1 FROM [FuelTransactions] WHERE [Id] = 3)
                    INSERT INTO [FuelTransactions] ([Id],[AuthorizedAmount],[CreatedAt],[PumpId],[RejectionReason],[RequestReference],[RequestedAmount],[StationId],[Status],[UpdatedAt],[WalletId])
                    VALUES (3, NULL, '2026-06-19T09:00:00Z', 3, 'Wallet balance is insufficient for this transaction.', 'SEED-REF-003', 300.00, 101, 'Rejected', '2026-06-19T09:00:00Z', 'WLT-1002');
                IF NOT EXISTS (SELECT 1 FROM [FuelTransactions] WHERE [Id] = 4)
                    INSERT INTO [FuelTransactions] ([Id],[AuthorizedAmount],[CreatedAt],[PumpId],[RejectionReason],[RequestReference],[RequestedAmount],[StationId],[Status],[UpdatedAt],[WalletId])
                    VALUES (4, 80.00, '2026-06-16T14:00:00Z', 1, NULL, 'SEED-REF-004', 80.00, 103, 'Authorized', '2026-06-16T14:00:01Z', 'WLT-1003');
                IF NOT EXISTS (SELECT 1 FROM [FuelTransactions] WHERE [Id] = 5)
                    INSERT INTO [FuelTransactions] ([Id],[AuthorizedAmount],[CreatedAt],[PumpId],[RejectionReason],[RequestReference],[RequestedAmount],[StationId],[Status],[UpdatedAt],[WalletId])
                    VALUES (5, NULL, '2026-06-15T11:00:00Z', 2, NULL, 'SEED-REF-005', 50.00, 101, 'Expired', '2026-06-15T11:02:00Z', 'WLT-1003');
                SET IDENTITY_INSERT [FuelTransactions] OFF;
            ");
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
