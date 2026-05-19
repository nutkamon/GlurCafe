using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace glur.cafe.page.Migrations
{
    /// <inheritdoc />
    public partial class AddCrmTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create Customers table (new)
            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FullName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Company = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Address = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CustomerType = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Source = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    Tags = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastContactedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });

            // Create CustomerInteractions table (new)
            migrationBuilder.CreateTable(
                name: "CustomerInteractions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CustomerId = table.Column<int>(type: "INTEGER", nullable: false),
                    Type = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Subject = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    FollowUpDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsFollowUpDone = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedByAdmin = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerInteractions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerInteractions_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Add CustomerId to existing Quotations
            migrationBuilder.AddColumn<int>(
                name: "CustomerId",
                table: "Quotations",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Quotations_CustomerId",
                table: "Quotations",
                column: "CustomerId");

            // Add CustomerId to existing ContactMessages
            migrationBuilder.AddColumn<int>(
                name: "CustomerId",
                table: "ContactMessages",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContactMessages_CustomerId",
                table: "ContactMessages",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Status",
                table: "Customers",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_CustomerType",
                table: "Customers",
                column: "CustomerType");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_CreatedAt",
                table: "Customers",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerInteractions_CustomerId",
                table: "CustomerInteractions",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerInteractions_FollowUpDate",
                table: "CustomerInteractions",
                column: "FollowUpDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "CustomerInteractions");
            migrationBuilder.DropTable(name: "Customers");

            migrationBuilder.DropIndex(name: "IX_Quotations_CustomerId", table: "Quotations");
            migrationBuilder.DropColumn(name: "CustomerId", table: "Quotations");

            migrationBuilder.DropIndex(name: "IX_ContactMessages_CustomerId", table: "ContactMessages");
            migrationBuilder.DropColumn(name: "CustomerId", table: "ContactMessages");
        }
    }
}
