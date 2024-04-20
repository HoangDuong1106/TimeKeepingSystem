using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObject.Migrations
{
    public partial class AddEmployeeNumber : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmployeeNumber",
                table: "Employees",
                type: "nvarchar(8)", // Make sure the size fits 'EM' + 6 digits
                nullable: true);

            // Use a SQL script to update each row uniquely
            // The SQL script will need to be crafted based on your DB's capabilities
            migrationBuilder.Sql(
                @"
        DECLARE @Counter INT = 1;
        UPDATE Employees SET EmployeeNumber = FORMAT(@Counter, 'EP000000') 
        WHERE EmployeeNumber IS NULL
        SET @Counter = @Counter + 1;
        ");

            // Alter column to non-nullable
            migrationBuilder.AlterColumn<string>(
                name: "EmployeeNumber",
                table: "Employees",
                type: "nvarchar(8)",
                nullable: false);

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmployeeNumber",
                table: "Employees");
        }

    }
}
