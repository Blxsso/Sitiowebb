using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sitiowebb.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUnavailability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Unavailabilities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserEmail = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Kind = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsHalfDay = table.Column<bool>(type: "INTEGER", nullable: false),
                    HalfSegment = table.Column<string>(type: "TEXT", maxLength: 2, nullable: true),
                    Justification = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Unavailabilities", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Unavailabilities");
        }
    }
}
