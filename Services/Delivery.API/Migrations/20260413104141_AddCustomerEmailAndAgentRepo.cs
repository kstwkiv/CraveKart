using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Delivery.API.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerEmailAndAgentRepo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomerEmail",
                table: "Deliveries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerEmail",
                table: "Deliveries");
        }
    }
}
