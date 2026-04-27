using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Delivery.API.Migrations
{
    /// <inheritdoc />
    public partial class AddTotalEarningsToAgent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TotalEarnings",
                table: "DeliveryAgents",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalEarnings",
                table: "DeliveryAgents");
        }
    }
}
