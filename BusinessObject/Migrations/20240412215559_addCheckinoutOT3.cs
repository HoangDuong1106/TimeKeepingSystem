using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObject.Migrations
{
    public partial class addCheckinoutOT3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CheckInTime",
                table: "RequestLeaves");

            migrationBuilder.DropColumn(
                name: "CheckOutTime",
                table: "RequestLeaves");

            migrationBuilder.AddColumn<string>(
                name: "CheckInTime",
                table: "RequestOverTimes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CheckOutTime",
                table: "RequestOverTimes",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CheckInTime",
                table: "RequestOverTimes");

            migrationBuilder.DropColumn(
                name: "CheckOutTime",
                table: "RequestOverTimes");

            migrationBuilder.AddColumn<string>(
                name: "CheckInTime",
                table: "RequestLeaves",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CheckOutTime",
                table: "RequestLeaves",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
