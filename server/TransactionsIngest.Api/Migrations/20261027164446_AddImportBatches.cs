using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransactionsIngest.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddImportBatches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ImportBatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StartedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false, defaultValue: "Running"),
                    InsertedCount = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    UpdatedCount = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    RevokedCount = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportBatches", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImportBatches");
        }
    }
}
