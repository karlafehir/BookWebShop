﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookWebShop.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class propchangeorderHeader : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OrderTime",
                table: "OrderHeaders",
                newName: "OrderDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OrderDate",
                table: "OrderHeaders",
                newName: "OrderTime");
        }
    }
}
