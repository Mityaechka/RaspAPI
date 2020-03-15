using Microsoft.EntityFrameworkCore.Migrations;

namespace RaspAPI.Migrations
{
    public partial class _3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShortName",
                table: "Facilities");

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "Facilities",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FullName",
                table: "Facilities");

            migrationBuilder.AddColumn<string>(
                name: "ShortName",
                table: "Facilities",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
