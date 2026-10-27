using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransactionsIngest.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddImportBatchUnchangedCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UnchangedCount",
                table: "ImportBatches",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnchangedCount",
                table: "ImportBatches");
        }
    }
}
