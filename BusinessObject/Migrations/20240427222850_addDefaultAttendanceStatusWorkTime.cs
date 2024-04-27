using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObject.Migrations
{
    public partial class addDefaultAttendanceStatusWorkTime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "attendanceStatusAfternoonId",
                table: "RequestWorkTimes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "attendanceStatusMorningId",
                table: "RequestWorkTimes",
                type: "uniqueidentifier",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "attendanceStatusAfternoonId",
                table: "RequestWorkTimes");

            migrationBuilder.DropColumn(
                name: "attendanceStatusMorningId",
                table: "RequestWorkTimes");
        }
    }
}
