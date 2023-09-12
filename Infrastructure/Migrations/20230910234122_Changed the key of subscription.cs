using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiPlugin.Migrations
{
    /// <inheritdoc />
    public partial class Changedthekeyofsubscription : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Delete the oldest rows for each SubscriptionId
            migrationBuilder.Sql(@"
                ;WITH CTE AS (
                    SELECT [SubscriptionId], [CreatedOn], 
                        ROW_NUMBER() OVER (PARTITION BY [SubscriptionId] ORDER BY [CreatedOn] DESC) as rn
                    FROM [Subscriptions]
                )
                DELETE FROM CTE WHERE rn > 1;
            ");

            // Step 2: Drop the existing primary key
            migrationBuilder.DropPrimaryKey(
                name: "PK_Subscriptions",
                table: "Subscriptions");

            // Step 3: Delete the old Id column
            migrationBuilder.DropColumn(
                name: "Id",
                table: "Subscriptions");

            // Step 4: Add new primary key constraint on SubscriptionId
            migrationBuilder.AddPrimaryKey(
                name: "PK_Subscriptions",
                table: "Subscriptions",
                column: "SubscriptionId");

            // Step 5: Rename SubscriptionId if you want
            migrationBuilder.RenameColumn(
                name: "SubscriptionId",
                table: "Subscriptions",
                newName: "Id");
        }


        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new NotImplementedException();
        }
    }
}
