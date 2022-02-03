using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoSats.Data.Migrations
{
    public partial class Notifications : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "ExchangeSchedules",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddColumn<string>(
                name: "Notification_Auth",
                table: "ExchangeSchedules",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notification_P256dh",
                table: "ExchangeSchedules",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Notification_Type",
                table: "ExchangeSchedules",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notification_Url",
                table: "ExchangeSchedules",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ApplicationSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PublicKey = table.Column<string>(type: "TEXT", nullable: false),
                    PrivateKey = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationSettings", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationSettings");

            migrationBuilder.DropColumn(
                name: "Notification_Auth",
                table: "ExchangeSchedules");

            migrationBuilder.DropColumn(
                name: "Notification_P256dh",
                table: "ExchangeSchedules");

            migrationBuilder.DropColumn(
                name: "Notification_Type",
                table: "ExchangeSchedules");

            migrationBuilder.DropColumn(
                name: "Notification_Url",
                table: "ExchangeSchedules");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "ExchangeSchedules",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Sqlite:Autoincrement", true);
        }
    }
}
