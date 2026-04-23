using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class AddTaxEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TaxCountries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CountryCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReferenceFxRate = table.Column<decimal>(type: "decimal(18,6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxCountries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TaxRegimes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FinancialYear = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Regime = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CessRate = table.Column<decimal>(type: "decimal(5,4)", nullable: false),
                    RebateThresholdUsd = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RebateAmountUsd = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxCountryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxRegimes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaxRegimes_TaxCountries_TaxCountryId",
                        column: x => x.TaxCountryId,
                        principalTable: "TaxCountries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaxSlabs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LowerBoundUsd = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UpperBoundUsd = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Rate = table.Column<decimal>(type: "decimal(5,4)", nullable: false),
                    TaxRegimeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxSlabs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaxSlabs_TaxRegimes_TaxRegimeId",
                        column: x => x.TaxRegimeId,
                        principalTable: "TaxRegimes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaxRegimes_TaxCountryId",
                table: "TaxRegimes",
                column: "TaxCountryId");

            migrationBuilder.CreateIndex(
                name: "IX_TaxSlabs_TaxRegimeId",
                table: "TaxSlabs",
                column: "TaxRegimeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaxSlabs");

            migrationBuilder.DropTable(
                name: "TaxRegimes");

            migrationBuilder.DropTable(
                name: "TaxCountries");
        }
    }
}