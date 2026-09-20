using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodOrderingApi.Migrations
{
    /// <inheritdoc />
    public partial class ProductCategoryAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "name",
                table: "Places",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "longtitude",
                table: "Places",
                newName: "Longtitude");

            migrationBuilder.RenameColumn(
                name: "latitude",
                table: "Places",
                newName: "Latitude");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "Places",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Places",
                newName: "Id");

            migrationBuilder.CreateTable(
                name: "ProductCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Hidden = table.Column<bool>(type: "INTEGER", nullable: false),
                    PlaceId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductCategories_Places_PlaceId",
                        column: x => x.PlaceId,
                        principalTable: "Places",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_PlaceId",
                table: "ProductCategories",
                column: "PlaceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductCategories");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Places",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Longtitude",
                table: "Places",
                newName: "longtitude");

            migrationBuilder.RenameColumn(
                name: "Latitude",
                table: "Places",
                newName: "latitude");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Places",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Places",
                newName: "id");
        }
    }
}
