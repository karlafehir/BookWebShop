using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookWebShop.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class orderHeaderprops : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Status",
                table: "OrderHeaders",
                newName: "State");

            migrationBuilder.AlterColumn<int>(
                name: "PostalCode",
                table: "OrderHeaders",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "State",
                table: "OrderHeaders",
                newName: "Status");

            migrationBuilder.AlterColumn<string>(
                name: "PostalCode",
                table: "OrderHeaders",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
