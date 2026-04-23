using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    EmpId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpName = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                    EmpEmail = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                    EmpDOB = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Password_Hash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.EmpId);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    AuditId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Action_Type = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                    Entity_Type = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                    Time_Stamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Employee_EmpId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.AuditId);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Employees_Employee_EmpId",
                        column: x => x.Employee_EmpId,
                        principalTable: "Employees",
                        principalColumn: "EmpId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Awards",
                columns: table => new
                {
                    AwardId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Award_Type = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                    Grant_Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Total_Units = table.Column<int>(type: "int", nullable: false),
                    Exercise_Price = table.Column<double>(type: "float", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                    Employee_EmpId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Awards", x => x.AwardId);
                    table.ForeignKey(
                        name: "FK_Awards_Employees_Employee_EmpId",
                        column: x => x.Employee_EmpId,
                        principalTable: "Employees",
                        principalColumn: "EmpId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    NotificationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Message = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                    Is_Read = table.Column<bool>(type: "bit", nullable: false),
                    Employee_EmpId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.NotificationId);
                    table.ForeignKey(
                        name: "FK_Notifications_Employees_Employee_EmpId",
                        column: x => x.Employee_EmpId,
                        principalTable: "Employees",
                        principalColumn: "EmpId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExerciseRequests",
                columns: table => new
                {
                    RequestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Units_Requested = table.Column<int>(type: "int", nullable: false),
                    Requested_Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                    Employee_EmpId = table.Column<int>(type: "int", nullable: false),
                    Awards_AwardId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseRequests", x => x.RequestId);
                    table.ForeignKey(
                        name: "FK_ExerciseRequests_Awards_Awards_AwardId",
                        column: x => x.Awards_AwardId,
                        principalTable: "Awards",
                        principalColumn: "AwardId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExerciseRequests_Employees_Employee_EmpId",
                        column: x => x.Employee_EmpId,
                        principalTable: "Employees",
                        principalColumn: "EmpId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VestingSchedules",
                columns: table => new
                {
                    Vesting_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Vesting_Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Units_Vested = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                    Employee_EmpId = table.Column<int>(type: "int", nullable: false),
                    Awards_AwardId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VestingSchedules", x => x.Vesting_Id);
                    table.ForeignKey(
                        name: "FK_VestingSchedules_Awards_Awards_AwardId",
                        column: x => x.Awards_AwardId,
                        principalTable: "Awards",
                        principalColumn: "AwardId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VestingSchedules_Employees_Employee_EmpId",
                        column: x => x.Employee_EmpId,
                        principalTable: "Employees",
                        principalColumn: "EmpId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Employee_EmpId",
                table: "AuditLogs",
                column: "Employee_EmpId");

            migrationBuilder.CreateIndex(
                name: "IX_Awards_Employee_EmpId",
                table: "Awards",
                column: "Employee_EmpId");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseRequests_Awards_AwardId",
                table: "ExerciseRequests",
                column: "Awards_AwardId");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseRequests_Employee_EmpId",
                table: "ExerciseRequests",
                column: "Employee_EmpId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Employee_EmpId",
                table: "Notifications",
                column: "Employee_EmpId");

            migrationBuilder.CreateIndex(
                name: "IX_VestingSchedules_Awards_AwardId",
                table: "VestingSchedules",
                column: "Awards_AwardId");

            migrationBuilder.CreateIndex(
                name: "IX_VestingSchedules_Employee_EmpId",
                table: "VestingSchedules",
                column: "Employee_EmpId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "ExerciseRequests");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "VestingSchedules");

            migrationBuilder.DropTable(
                name: "Awards");

            migrationBuilder.DropTable(
                name: "Employees");
        }
    }
}
