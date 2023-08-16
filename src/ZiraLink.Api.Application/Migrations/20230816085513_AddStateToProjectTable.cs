using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZiraLink.Api.Application.Migrations
{
    /// <inheritdoc />
    public partial class AddStateToProjectTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "State",
                table: "Projects",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "State",
                table: "Projects");
        }
    }
}
