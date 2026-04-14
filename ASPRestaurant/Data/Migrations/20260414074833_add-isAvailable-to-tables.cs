using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASPRestaurant.Data.Migrations
{
    /// <inheritdoc />
    public partial class addisAvailabletotables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAvailable",
                table: "Tables",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAvailable",
                table: "Tables");
        }
    }
}
