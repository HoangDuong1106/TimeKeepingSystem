using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObject.Migrations
{
    public partial class removeDepartmentIdInHoliday : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DepartmentHolidays_Departments_DepartmentId",
                table: "DepartmentHolidays");

            migrationBuilder.DropIndex(
                name: "IX_DepartmentHolidays_DepartmentId",
                table: "DepartmentHolidays");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "DepartmentHolidays");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DepartmentId",
                table: "DepartmentHolidays",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentHolidays_DepartmentId",
                table: "DepartmentHolidays",
                column: "DepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_DepartmentHolidays_Departments_DepartmentId",
                table: "DepartmentHolidays",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
