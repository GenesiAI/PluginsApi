using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiPlugin.Migrations
{
    /// <inheritdoc />
    public partial class Improvementsonsubscriptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.RenameColumn(
                name: "EventTime",
                table: "Subscriptions",
                newName: "CreatedOn");

            migrationBuilder.RenameColumn(
                name: "SubscriptionId",
                table: "Subscriptions",
                newName: "Id");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Plugins",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Plugins");

            migrationBuilder.RenameColumn(
                name: "CreatedOn",
                table: "Subscriptions",
                newName: "EventTime");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Subscriptions",
                newName: "SubscriptionId");

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CustomerId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.UserId);
                });
        }
    }
}
