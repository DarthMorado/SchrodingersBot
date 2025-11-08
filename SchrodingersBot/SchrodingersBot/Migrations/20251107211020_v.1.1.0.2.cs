using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchrodingersBot.Migrations
{
    /// <inheritdoc />
    public partial class v1102 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "GameSubscriptions",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "GameSubscriptions");
        }
    }
}
