using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sitiowebb.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddManagerCommentToVacationRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DecidedByManagerId",
                table: "VacationRequests");

            migrationBuilder.RenameColumn(
                name: "DecidedByManagerName",
                table: "VacationRequests",
                newName: "ManagerComment");

            migrationBuilder.AlterColumn<string>(
                name: "UserEmail",
                table: "VacationRequests",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ManagerComment",
                table: "VacationRequests",
                newName: "DecidedByManagerName");

            migrationBuilder.AlterColumn<string>(
                name: "UserEmail",
                table: "VacationRequests",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DecidedByManagerId",
                table: "VacationRequests",
                type: "TEXT",
                nullable: true);
        }
    }
}
