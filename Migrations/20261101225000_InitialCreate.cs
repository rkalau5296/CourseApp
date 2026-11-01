using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CourseApp.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CourseNodes",
                columns: table => new
                {
                    CourseNodeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseNodes", x => x.CourseNodeId);
                    table.ForeignKey(
                        name: "FK_CourseNodes_CourseNodes_ParentId",
                        column: x => x.ParentId,
                        principalTable: "CourseNodes",
                        principalColumn: "CourseNodeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CourseItems",
                columns: table => new
                {
                    CourseItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CourseNodeId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseItems", x => x.CourseItemId);
                    table.ForeignKey(
                        name: "FK_CourseItems_CourseNodes_CourseNodeId",
                        column: x => x.CourseNodeId,
                        principalTable: "CourseNodes",
                        principalColumn: "CourseNodeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CourseItems_CourseNodeId",
                table: "CourseItems",
                column: "CourseNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseNodes_ParentId",
                table: "CourseNodes",
                column: "ParentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CourseItems");

            migrationBuilder.DropTable(
                name: "CourseNodes");
        }
    }
}
