using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiPlugin.Migrations
{
    /// <inheritdoc />
    public partial class ChangedSubscriptionIdAndRemvedCheckout : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Checkouts");

            //drop the constraint on the old primary key
            migrationBuilder.DropPrimaryKey(
                name: "PK_Subscriptions",
                table: "Subscriptions");

            //changed the name of id to SubscriptionId
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Subscriptions",
                newName: "SubscriptionId");

            //added a new column of type Guid id
            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "Subscriptions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");  // Generate new GUIDs for existing records


            // make the new column the primary key and the old column a normal column
            migrationBuilder.AddPrimaryKey(
                name: "PK_Subscriptions",
                table: "Subscriptions",
                column: "Id");


            //old generatd code
            // migrationBuilder.AlterColumn<Guid>(
            //     name: "Id",
            //     table: "Subscriptions",
            //     type: "uniqueidentifier",
            //     nullable: false,
            //     oldClrType: typeof(string),
            //     oldType: "nvarchar(450)");

            // migrationBuilder.AddColumn<string>(
            //     name: "SubscriptionId",
            //     table: "Subscriptions",
            //     type: "nvarchar(max)",
            //     nullable: false,
            //     defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the new primary key constraint
            migrationBuilder.DropPrimaryKey(
                name: "PK_Subscriptions",
                table: "Subscriptions");

            // Drop the new Id column
            migrationBuilder.DropColumn(
                name: "Id",
                table: "Subscriptions");

            // Rename 'SubscriptionId' back to 'Id'
            migrationBuilder.RenameColumn(
                name: "SubscriptionId",
                table: "Subscriptions",
                newName: "Id");

            // Add back the original primary key
            migrationBuilder.AddPrimaryKey(
                name: "PK_Subscriptions",
                table: "Subscriptions",
                column: "Id");

            // Re-create the 'Checkouts' table
            migrationBuilder.CreateTable(
                name: "Checkouts",
                columns: table => new
                {
                    CheckoutSessionId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Checkouts", x => x.CheckoutSessionId);
                });
        }

    }
}
