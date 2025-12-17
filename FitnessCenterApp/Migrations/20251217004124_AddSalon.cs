using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitnessCenterApp.Migrations
{
    /// <inheritdoc />
    public partial class AddSalon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Antrenorler_Salonlar_SalonId",
                table: "Antrenorler");

            migrationBuilder.DropIndex(
                name: "IX_Antrenorler_SalonId",
                table: "Antrenorler");

            migrationBuilder.DropColumn(
                name: "CalismaSaatleri",
                table: "Salonlar");

            migrationBuilder.DropColumn(
                name: "SalonId",
                table: "Antrenorler");

            migrationBuilder.AlterColumn<string>(
                name: "Telefon",
                table: "Salonlar",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Adres",
                table: "Salonlar",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Ad",
                table: "Salonlar",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<bool>(
                name: "Aktif",
                table: "Salonlar",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Sehir",
                table: "Salonlar",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Aktif",
                table: "Salonlar");

            migrationBuilder.DropColumn(
                name: "Sehir",
                table: "Salonlar");

            migrationBuilder.AlterColumn<string>(
                name: "Telefon",
                table: "Salonlar",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Adres",
                table: "Salonlar",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Ad",
                table: "Salonlar",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "CalismaSaatleri",
                table: "Salonlar",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "SalonId",
                table: "Antrenorler",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Antrenorler_SalonId",
                table: "Antrenorler",
                column: "SalonId");

            migrationBuilder.AddForeignKey(
                name: "FK_Antrenorler_Salonlar_SalonId",
                table: "Antrenorler",
                column: "SalonId",
                principalTable: "Salonlar",
                principalColumn: "Id");
        }
    }
}
