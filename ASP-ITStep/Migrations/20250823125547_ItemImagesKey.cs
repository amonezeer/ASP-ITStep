using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASP_ITStep.Migrations
{
    /// <inheritdoc />
    public partial class ItemImagesKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Items",
                table: "Items");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Items",
                newName: "ItemId");

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "ProductGroups",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "Items",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Items",
                table: "Items",
                columns: new[] { "ItemId", "ImageUrl" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductGroups_Slug",
                table: "ProductGroups",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductGroups_Slug",
                table: "ProductGroups");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Items",
                table: "Items");

            migrationBuilder.RenameColumn(
                name: "ItemId",
                table: "Items",
                newName: "Id");

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "ProductGroups",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "Items",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Items",
                table: "Items",
                column: "Id");
        }
    }
}
