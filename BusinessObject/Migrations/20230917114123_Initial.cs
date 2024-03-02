using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObject.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LeaveSettings",
                columns: table => new
                {
                    LeaveSettingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaxDateLeave = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsManagerAssigned = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveSettings", x => x.LeaveSettingId);
                });

            migrationBuilder.CreateTable(
                name: "LeaveTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AllowedDays = table.Column<int>(type: "int", nullable: true),
                    LeaveCycle = table.Column<int>(type: "int", nullable: false),
                    CanCarryForward = table.Column<bool>(type: "bit", nullable: false),
                    TotalBalance = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RiskPerformanceSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Hours = table.Column<int>(type: "int", nullable: false),
                    Days = table.Column<int>(type: "int", nullable: false),
                    DateSet = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskPerformanceSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Wifis",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BSSID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wifis", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkDateSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DateStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkDateSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkingStatuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkingStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkPermissionSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkPermissionSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkTimeSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromHourMorning = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ToHourMorning = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FromHourAfternoon = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ToHourAfternoon = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkTimeSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RequestLeaves",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LeaveTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ToDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestLeaves", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RequestLeaves_LeaveTypes_LeaveTypeId",
                        column: x => x.LeaveTypeId,
                        principalTable: "LeaveTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AttendanceStatuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LeaveTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    WorkingStatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceStatuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttendanceStatuses_LeaveTypes_LeaveTypeId",
                        column: x => x.LeaveTypeId,
                        principalTable: "LeaveTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AttendanceStatuses_WorkingStatuses_WorkingStatusId",
                        column: x => x.WorkingStatusId,
                        principalTable: "WorkingStatuses",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RequestOverTimes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DateOfOverTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FromHour = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NumberOfHour = table.Column<double>(type: "float", nullable: false),
                    ToHour = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WorkingStatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestOverTimes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RequestOverTimes_WorkingStatuses_WorkingStatusId",
                        column: x => x.WorkingStatusId,
                        principalTable: "WorkingStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkTrackSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkTimeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    WorkDateId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RiskPerfomanceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LeaveSettingId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkTrackSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkTrackSettings_LeaveSettings_LeaveSettingId",
                        column: x => x.LeaveSettingId,
                        principalTable: "LeaveSettings",
                        principalColumn: "LeaveSettingId");
                    table.ForeignKey(
                        name: "FK_WorkTrackSettings_RiskPerformanceSettings_RiskPerfomanceId",
                        column: x => x.RiskPerfomanceId,
                        principalTable: "RiskPerformanceSettings",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WorkTrackSettings_WorkDateSettings_WorkDateId",
                        column: x => x.WorkDateId,
                        principalTable: "WorkDateSettings",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WorkTrackSettings_WorkTimeSettings_WorkTimeId",
                        column: x => x.WorkTimeId,
                        principalTable: "WorkTimeSettings",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ManagerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WorkTrackId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Departments_WorkTrackSettings_WorkTrackId",
                        column: x => x.WorkTrackId,
                        principalTable: "WorkTrackSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DepartmentHolidays",
                columns: table => new
                {
                    HolidayId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HolidayName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsRecurring = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepartmentHolidays", x => x.HolidayId);
                    table.ForeignKey(
                        name: "FK_DepartmentHolidays_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Gender = table.Column<bool>(type: "bit", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    EmployeeStatus = table.Column<int>(type: "int", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Employees_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Workslots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsMorning = table.Column<bool>(type: "bit", nullable: false),
                    DateOfSlot = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FromHour = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ToHour = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workslots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Workslots_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DepartmentHolidayExceptions",
                columns: table => new
                {
                    ExceptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HolidayId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExceptionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepartmentHolidayExceptions", x => x.ExceptionId);
                    table.ForeignKey(
                        name: "FK_DepartmentHolidayExceptions_DepartmentHolidays_HolidayId",
                        column: x => x.HolidayId,
                        principalTable: "DepartmentHolidays",
                        principalColumn: "HolidayId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RiskPerformanceEmployees",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RiskPerformanceSettingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ViolationJSON = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskPerformanceEmployees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RiskPerformanceEmployees_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RiskPerformanceEmployees_RiskPerformanceSettings_RiskPerformanceSettingId",
                        column: x => x.RiskPerformanceSettingId,
                        principalTable: "RiskPerformanceSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserAccounts",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SaltPassword = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAccounts", x => x.ID);
                    table.ForeignKey(
                        name: "FK_UserAccounts_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserAccounts_Roles_RoleID",
                        column: x => x.RoleID,
                        principalTable: "Roles",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkslotEmployees",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CheckInTime = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CheckOutTime = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkslotId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AttendanceStatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RequestLeaveId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkslotEmployees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkslotEmployees_AttendanceStatuses_AttendanceStatusId",
                        column: x => x.AttendanceStatusId,
                        principalTable: "AttendanceStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkslotEmployees_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkslotEmployees_RequestLeaves_RequestLeaveId",
                        column: x => x.RequestLeaveId,
                        principalTable: "RequestLeaves",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WorkslotEmployees_Workslots_WorkslotId",
                        column: x => x.WorkslotId,
                        principalTable: "Workslots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RequestWorkTimes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RealHourStart = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RealHourEnd = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateOfSlot = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NumberOfComeLateHour = table.Column<float>(type: "real", nullable: true),
                    NumberOfLeaveEarlyHour = table.Column<float>(type: "real", nullable: true),
                    WorkslotEmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestWorkTimes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RequestWorkTimes_WorkslotEmployees_WorkslotEmployeeId",
                        column: x => x.WorkslotEmployeeId,
                        principalTable: "WorkslotEmployees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestLeaveId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RequestWorkTimeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EmployeeSendRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    PathAttachmentFile = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    SubmitedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RequestOverTimeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    requestType = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", maxLength: 20, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Requests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Requests_Employees_EmployeeSendRequestId",
                        column: x => x.EmployeeSendRequestId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Requests_RequestLeaves_RequestLeaveId",
                        column: x => x.RequestLeaveId,
                        principalTable: "RequestLeaves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Requests_RequestOverTimes_RequestOverTimeId",
                        column: x => x.RequestOverTimeId,
                        principalTable: "RequestOverTimes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Requests_RequestWorkTimes_RequestWorkTimeId",
                        column: x => x.RequestWorkTimeId,
                        principalTable: "RequestWorkTimes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceStatuses_LeaveTypeId",
                table: "AttendanceStatuses",
                column: "LeaveTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceStatuses_WorkingStatusId",
                table: "AttendanceStatuses",
                column: "WorkingStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentHolidayExceptions_HolidayId",
                table: "DepartmentHolidayExceptions",
                column: "HolidayId");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentHolidays_DepartmentId",
                table: "DepartmentHolidays",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_WorkTrackId",
                table: "Departments",
                column: "WorkTrackId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_DepartmentId",
                table: "Employees",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestLeaves_LeaveTypeId",
                table: "RequestLeaves",
                column: "LeaveTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestOverTimes_WorkingStatusId",
                table: "RequestOverTimes",
                column: "WorkingStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_EmployeeSendRequestId",
                table: "Requests",
                column: "EmployeeSendRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_RequestLeaveId",
                table: "Requests",
                column: "RequestLeaveId");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_RequestOverTimeId",
                table: "Requests",
                column: "RequestOverTimeId");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_RequestWorkTimeId",
                table: "Requests",
                column: "RequestWorkTimeId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestWorkTimes_WorkslotEmployeeId",
                table: "RequestWorkTimes",
                column: "WorkslotEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_RiskPerformanceEmployees_EmployeeId",
                table: "RiskPerformanceEmployees",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_RiskPerformanceEmployees_RiskPerformanceSettingId",
                table: "RiskPerformanceEmployees",
                column: "RiskPerformanceSettingId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAccounts_EmployeeId",
                table: "UserAccounts",
                column: "EmployeeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserAccounts_RoleID",
                table: "UserAccounts",
                column: "RoleID");

            migrationBuilder.CreateIndex(
                name: "IX_WorkslotEmployees_AttendanceStatusId",
                table: "WorkslotEmployees",
                column: "AttendanceStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkslotEmployees_EmployeeId",
                table: "WorkslotEmployees",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkslotEmployees_RequestLeaveId",
                table: "WorkslotEmployees",
                column: "RequestLeaveId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkslotEmployees_WorkslotId",
                table: "WorkslotEmployees",
                column: "WorkslotId");

            migrationBuilder.CreateIndex(
                name: "IX_Workslots_DepartmentId",
                table: "Workslots",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkTrackSettings_LeaveSettingId",
                table: "WorkTrackSettings",
                column: "LeaveSettingId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkTrackSettings_RiskPerfomanceId",
                table: "WorkTrackSettings",
                column: "RiskPerfomanceId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkTrackSettings_WorkDateId",
                table: "WorkTrackSettings",
                column: "WorkDateId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkTrackSettings_WorkTimeId",
                table: "WorkTrackSettings",
                column: "WorkTimeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DepartmentHolidayExceptions");

            migrationBuilder.DropTable(
                name: "Requests");

            migrationBuilder.DropTable(
                name: "RiskPerformanceEmployees");

            migrationBuilder.DropTable(
                name: "UserAccounts");

            migrationBuilder.DropTable(
                name: "Wifis");

            migrationBuilder.DropTable(
                name: "WorkPermissionSettings");

            migrationBuilder.DropTable(
                name: "DepartmentHolidays");

            migrationBuilder.DropTable(
                name: "RequestOverTimes");

            migrationBuilder.DropTable(
                name: "RequestWorkTimes");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "WorkslotEmployees");

            migrationBuilder.DropTable(
                name: "AttendanceStatuses");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "RequestLeaves");

            migrationBuilder.DropTable(
                name: "Workslots");

            migrationBuilder.DropTable(
                name: "WorkingStatuses");

            migrationBuilder.DropTable(
                name: "LeaveTypes");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "WorkTrackSettings");

            migrationBuilder.DropTable(
                name: "LeaveSettings");

            migrationBuilder.DropTable(
                name: "RiskPerformanceSettings");

            migrationBuilder.DropTable(
                name: "WorkDateSettings");

            migrationBuilder.DropTable(
                name: "WorkTimeSettings");
        }
    }
}
