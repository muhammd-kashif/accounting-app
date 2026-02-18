using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountingApp.Migrations
{
    /// <inheritdoc />
    public partial class LinkSalesToProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Clear existing sales data to avoid FK conflicts
            migrationBuilder.Sql("DELETE FROM SalePayments");
            migrationBuilder.Sql("DELETE FROM Invoices");
            migrationBuilder.Sql("DELETE FROM SaleItems");
            migrationBuilder.Sql("DELETE FROM Sales");

            migrationBuilder.DropForeignKey(
                name: "FK_SaleItems_Items_ItemId",
                table: "SaleItems");

            migrationBuilder.RenameColumn(
                name: "ItemId",
                table: "SaleItems",
                newName: "ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_SaleItems_ItemId",
                table: "SaleItems",
                newName: "IX_SaleItems_ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_SaleItems_Products_ProductId",
                table: "SaleItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SaleItems_Products_ProductId",
                table: "SaleItems");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "SaleItems",
                newName: "ItemId");

            migrationBuilder.RenameIndex(
                name: "IX_SaleItems_ProductId",
                table: "SaleItems",
                newName: "IX_SaleItems_ItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_SaleItems_Items_ItemId",
                table: "SaleItems",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
