using Microsoft.EntityFrameworkCore.Migrations;

namespace RaspAPI.Migrations
{
    public partial class _2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MainUrl",
                table: "Facilities",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ScheduleUrl",
                table: "Facilities",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShortName",
                table: "Facilities",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MainUrl",
                table: "Facilities");

            migrationBuilder.DropColumn(
                name: "ScheduleUrl",
                table: "Facilities");

            migrationBuilder.DropColumn(
                name: "ShortName",
                table: "Facilities");
        }
    }
}
