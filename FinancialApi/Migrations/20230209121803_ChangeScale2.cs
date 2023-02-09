using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinancialApi.Migrations
{
    /// <inheritdoc />
    public partial class ChangeScale2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Value",
                table: "Currencies",
                type: "decimal(38,15)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,0)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Value",
                table: "Currencies",
                type: "decimal(18,0)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(38,15)");
        }
    }
}
