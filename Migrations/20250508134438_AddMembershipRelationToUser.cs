using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymApp_v1.Migrations
{
    /// <inheritdoc />
    public partial class AddMembershipRelationToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MembershipId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_MembershipId",
                table: "Users",
                column: "MembershipId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Memberships_MembershipId",
                table: "Users",
                column: "MembershipId",
                principalTable: "Memberships",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Memberships_MembershipId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_MembershipId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "MembershipId",
                table: "Users");
        }
    }
}
