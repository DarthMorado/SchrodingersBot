using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchrodingersBot.Migrations
{
    /// <inheritdoc />
    public partial class v002 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Distance",
                table: "Areas");

            migrationBuilder.AlterColumn<int>(
                name: "ChatId",
                table: "Areas",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<double>(
                name: "CenterLat",
                table: "Areas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "CenterLon",
                table: "Areas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PolygonJson",
                table: "Areas",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "RadiusInMeters",
                table: "Areas",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CenterLat",
                table: "Areas");

            migrationBuilder.DropColumn(
                name: "CenterLon",
                table: "Areas");

            migrationBuilder.DropColumn(
                name: "PolygonJson",
                table: "Areas");

            migrationBuilder.DropColumn(
                name: "RadiusInMeters",
                table: "Areas");

            migrationBuilder.AlterColumn<int>(
                name: "ChatId",
                table: "Areas",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "Distance",
                table: "Areas",
                type: "int",
                nullable: true);
        }
    }
}
