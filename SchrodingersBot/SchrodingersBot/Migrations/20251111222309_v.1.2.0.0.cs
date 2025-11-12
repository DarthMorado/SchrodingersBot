using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchrodingersBot.Migrations
{
    /// <inheritdoc />
    public partial class v1200 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BrowserCookiesJson",
                table: "LoginInfos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Domain",
                table: "LoginInfos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GameId",
                table: "LoginInfos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BrowserCookiesJson",
                table: "LoginInfos");

            migrationBuilder.DropColumn(
                name: "Domain",
                table: "LoginInfos");

            migrationBuilder.DropColumn(
                name: "GameId",
                table: "LoginInfos");
        }
    }
}
