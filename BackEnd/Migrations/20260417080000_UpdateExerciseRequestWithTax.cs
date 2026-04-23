using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class UpdateExerciseRequestWithTax : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CurrentSharePrice",
                table: "ExerciseRequests",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ExerciseAmountUsd",
                table: "ExerciseRequests",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "NetAmountUsd",
                table: "ExerciseRequests",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxAmountUsd",
                table: "ExerciseRequests",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxableGainUsd",
                table: "ExerciseRequests",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentSharePrice",
                table: "ExerciseRequests");

            migrationBuilder.DropColumn(
                name: "ExerciseAmountUsd",
                table: "ExerciseRequests");

            migrationBuilder.DropColumn(
                name: "NetAmountUsd",
                table: "ExerciseRequests");

            migrationBuilder.DropColumn(
                name: "TaxAmountUsd",
                table: "ExerciseRequests");

            migrationBuilder.DropColumn(
                name: "TaxableGainUsd",
                table: "ExerciseRequests");
        }
    }
}