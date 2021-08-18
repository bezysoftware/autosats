using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AutoSats.Data.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExchangeSchedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Exchange = table.Column<string>(type: "TEXT", nullable: false),
                    Cron = table.Column<string>(type: "TEXT", nullable: false),
                    IsPaused = table.Column<bool>(type: "INTEGER", nullable: false),
                    Spend = table.Column<decimal>(type: "TEXT", nullable: false),
                    CurrencyPair = table.Column<string>(type: "TEXT", nullable: false),
                    WithdrawalType = table.Column<int>(type: "INTEGER", nullable: false),
                    WithdrawalAddress = table.Column<string>(type: "TEXT", nullable: true),
                    WithdrawalAmount = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExchangeSchedules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExchangeEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ScheduleId = table.Column<int>(type: "INTEGER", nullable: false),
                    Error = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExchangeEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExchangeEvents_ExchangeSchedules_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "ExchangeSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExchangeBuys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrderId = table.Column<string>(type: "TEXT", nullable: true),
                    Price = table.Column<decimal>(type: "TEXT", nullable: false),
                    Received = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExchangeBuys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExchangeBuys_ExchangeEvents_Id",
                        column: x => x.Id,
                        principalTable: "ExchangeEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExchangeWithdrawals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WithdrawalId = table.Column<string>(type: "TEXT", nullable: true),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    Address = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExchangeWithdrawals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExchangeWithdrawals_ExchangeEvents_Id",
                        column: x => x.Id,
                        principalTable: "ExchangeEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeEvents_ScheduleId",
                table: "ExchangeEvents",
                column: "ScheduleId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExchangeBuys");

            migrationBuilder.DropTable(
                name: "ExchangeWithdrawals");

            migrationBuilder.DropTable(
                name: "ExchangeEvents");

            migrationBuilder.DropTable(
                name: "ExchangeSchedules");
        }
    }
}
