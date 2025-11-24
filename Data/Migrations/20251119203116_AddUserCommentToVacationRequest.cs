using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sitiowebb.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserCommentToVacationRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserComment",
                table: "VacationRequests",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserComment",
                table: "VacationRequests");
        }
    }
}
