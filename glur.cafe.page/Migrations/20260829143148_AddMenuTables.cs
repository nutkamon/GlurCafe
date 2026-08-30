using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace glur.cafe.page.Migrations
{
    /// <inheritdoc />
    public partial class AddMenuTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MenuFolders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    IconClassName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuFolders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MessengerConversations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PsId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    LastMessageText = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    LastMessageAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UnreadCount = table.Column<int>(type: "INTEGER", nullable: false),
                    CustomerId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessengerConversations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MenuItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FolderId = table.Column<int>(type: "INTEGER", nullable: false),
                    FilePath = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    MediaType = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Price = table.Column<decimal>(type: "TEXT", nullable: true),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MenuItems_MenuFolders_FolderId",
                        column: x => x.FolderId,
                        principalTable: "MenuFolders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MessengerMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ConversationId = table.Column<int>(type: "INTEGER", nullable: false),
                    MessageId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    IsFromPage = table.Column<bool>(type: "INTEGER", nullable: false),
                    Text = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    SentAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsRead = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessengerMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MessengerMessages_MessengerConversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "MessengerConversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MenuFolders_DisplayOrder",
                table: "MenuFolders",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_DisplayOrder",
                table: "MenuItems",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_FolderId",
                table: "MenuItems",
                column: "FolderId");

            migrationBuilder.CreateIndex(
                name: "IX_MessengerConversations_LastMessageAt",
                table: "MessengerConversations",
                column: "LastMessageAt");

            migrationBuilder.CreateIndex(
                name: "IX_MessengerConversations_PsId",
                table: "MessengerConversations",
                column: "PsId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MessengerMessages_ConversationId",
                table: "MessengerMessages",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_MessengerMessages_MessageId",
                table: "MessengerMessages",
                column: "MessageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MenuItems");

            migrationBuilder.DropTable(
                name: "MessengerMessages");

            migrationBuilder.DropTable(
                name: "MenuFolders");

            migrationBuilder.DropTable(
                name: "MessengerConversations");
        }
    }
}
