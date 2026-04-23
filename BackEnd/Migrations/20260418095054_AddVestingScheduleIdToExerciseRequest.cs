using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class AddVestingScheduleIdToExerciseRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VestingScheduleId",
                table: "ExerciseRequests",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseRequests_VestingScheduleId",
                table: "ExerciseRequests",
                column: "VestingScheduleId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExerciseRequests_VestingSchedules_VestingScheduleId",
                table: "ExerciseRequests",
                column: "VestingScheduleId",
                principalTable: "VestingSchedules",
                principalColumn: "Vesting_Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExerciseRequests_VestingSchedules_VestingScheduleId",
                table: "ExerciseRequests");

            migrationBuilder.DropIndex(
                name: "IX_ExerciseRequests_VestingScheduleId",
                table: "ExerciseRequests");

            migrationBuilder.DropColumn(
                name: "VestingScheduleId",
                table: "ExerciseRequests");
        }
    }
}
