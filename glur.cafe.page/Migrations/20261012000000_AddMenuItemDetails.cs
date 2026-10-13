using glur.cafe.page.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace glur.cafe.page.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261012000000_AddMenuItemDetails")]
    public partial class AddMenuItemDetails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "MenuItems",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Options",
                table: "MenuItems",
                type: "TEXT",
                maxLength: 500,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Description", table: "MenuItems");
            migrationBuilder.DropColumn(name: "Options", table: "MenuItems");
        }
    }
}