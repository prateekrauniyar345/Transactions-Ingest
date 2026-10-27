using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransactionsIngest.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddTransactionDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CardNumber",
                table: "Transactions",
                type: "TEXT",
                maxLength: 19,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LocationCode",
                table: "Transactions",
                type: "TEXT",
                maxLength: 19,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProductName",
                table: "Transactions",
                type: "TEXT",
                maxLength: 19,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CardNumber",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "LocationCode",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "ProductName",
                table: "Transactions");
        }
    }
}
