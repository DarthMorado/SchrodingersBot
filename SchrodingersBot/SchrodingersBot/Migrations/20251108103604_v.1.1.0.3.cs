using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchrodingersBot.Migrations
{
    /// <inheritdoc />
    public partial class v1103 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ActiveLevelId",
                table: "GameSubscriptions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ActiveLevelNumber",
                table: "GameSubscriptions",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActiveLevelId",
                table: "GameSubscriptions");

            migrationBuilder.DropColumn(
                name: "ActiveLevelNumber",
                table: "GameSubscriptions");
        }
    }
}
