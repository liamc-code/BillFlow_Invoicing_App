using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InvoicingApp.Migrations
{
    public partial class AddDeletedAtColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Customers",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Customers");
        }
    }
}
