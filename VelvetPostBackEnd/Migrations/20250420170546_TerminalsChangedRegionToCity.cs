using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VelvetPostBackEnd.Migrations
{
    /// <inheritdoc />
    public partial class TerminalsChangedRegionToCity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Region",
                table: "Terminals",
                newName: "City");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "City",
                table: "Terminals",
                newName: "Region");
        }
    }
}
