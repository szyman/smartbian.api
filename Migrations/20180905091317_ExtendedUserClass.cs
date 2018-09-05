using Microsoft.EntityFrameworkCore.Migrations;

namespace SmartRoomsApp.API.Migrations
{
    public partial class ExtendedUserClass : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RaspHost",
                table: "Users",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RaspUsername",
                table: "Users",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Blocks",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DataX = table.Column<int>(nullable: false),
                    DataY = table.Column<int>(nullable: false),
                    Width = table.Column<string>(nullable: true),
                    UserId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Blocks_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Blocks_UserId",
                table: "Blocks",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Blocks");

            migrationBuilder.DropColumn(
                name: "RaspHost",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RaspUsername",
                table: "Users");
        }
    }
}
