using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObject.Migrations
{
    public partial class addDefaultDateMaxDateLeave : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "MaxDateLeaves",
                table: "WorkTrackSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "\"[\\r\\n  {\\r\\n    \\u0027Year\\u0027: 2023,\\r\\n    \\u0027LeaveTypeMaxDays\\u0027: {\\r\\n      \\u0027790F290E-4CBD-11EE-BE56-0242AC120002\\u0027: 5,\\r\\n      \\u0027790F2378-4CBD-11EE-BE56-0242AC120002\\u0027: 7,\\r\\n      \\u0027790F277E-4CBD-11EE-BE56-0242AC120002\\u0027: 3,\\r\\n      \\u0027790F24A4-4CBD-11EE-BE56-0242AC120002\\u0027: 9,\\r\\n      \\u0027790F20A8-4CBD-11EE-BE56-0242AC120002\\u0027: 10,\\r\\n      \\u0027790F25C6-4CBD-11EE-BE56-0242AC120002\\u0027: 4\\r\\n    }\\r\\n  },\\r\\n  {\\r\\n    \\u0027Year\\u0027: 2024,\\r\\n    \\u0027LeaveTypeMaxDays\\u0027: {\\r\\n      \\u0027790F290E-4CBD-11EE-BE56-0242AC120002\\u0027: 6,\\r\\n      \\u0027790F2378-4CBD-11EE-BE56-0242AC120002\\u0027: 8,\\r\\n      \\u0027790F277E-4CBD-11EE-BE56-0242AC120002\\u0027: 2,\\r\\n      \\u0027790F24A4-4CBD-11EE-BE56-0242AC120002\\u0027: 7,\\r\\n      \\u0027790F20A8-4CBD-11EE-BE56-0242AC120002\\u0027: 9,\\r\\n      \\u0027790F25C6-4CBD-11EE-BE56-0242AC120002\\u0027: 5\\r\\n    }\\r\\n  }\\r\\n]\\r\\n\"",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldDefaultValue: "[]");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "MaxDateLeaves",
                table: "WorkTrackSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldDefaultValue: "\"[\\r\\n  {\\r\\n    \\u0027Year\\u0027: 2023,\\r\\n    \\u0027LeaveTypeMaxDays\\u0027: {\\r\\n      \\u0027790F290E-4CBD-11EE-BE56-0242AC120002\\u0027: 5,\\r\\n      \\u0027790F2378-4CBD-11EE-BE56-0242AC120002\\u0027: 7,\\r\\n      \\u0027790F277E-4CBD-11EE-BE56-0242AC120002\\u0027: 3,\\r\\n      \\u0027790F24A4-4CBD-11EE-BE56-0242AC120002\\u0027: 9,\\r\\n      \\u0027790F20A8-4CBD-11EE-BE56-0242AC120002\\u0027: 10,\\r\\n      \\u0027790F25C6-4CBD-11EE-BE56-0242AC120002\\u0027: 4\\r\\n    }\\r\\n  },\\r\\n  {\\r\\n    \\u0027Year\\u0027: 2024,\\r\\n    \\u0027LeaveTypeMaxDays\\u0027: {\\r\\n      \\u0027790F290E-4CBD-11EE-BE56-0242AC120002\\u0027: 6,\\r\\n      \\u0027790F2378-4CBD-11EE-BE56-0242AC120002\\u0027: 8,\\r\\n      \\u0027790F277E-4CBD-11EE-BE56-0242AC120002\\u0027: 2,\\r\\n      \\u0027790F24A4-4CBD-11EE-BE56-0242AC120002\\u0027: 7,\\r\\n      \\u0027790F20A8-4CBD-11EE-BE56-0242AC120002\\u0027: 9,\\r\\n      \\u0027790F25C6-4CBD-11EE-BE56-0242AC120002\\u0027: 5\\r\\n    }\\r\\n  }\\r\\n]\\r\\n\"");
        }
    }
}
