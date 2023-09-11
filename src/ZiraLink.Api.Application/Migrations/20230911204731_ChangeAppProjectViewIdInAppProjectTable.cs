using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZiraLink.Api.Application.Migrations
{
    /// <inheritdoc />
    public partial class ChangeAppProjectViewIdInAppProjectTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProjectViewId",
                table: "AppProjects",
                newName: "ViewId");

            migrationBuilder.RenameColumn(
                name: "AppUniqueName",
                table: "AppProjects",
                newName: "AppProjectViewId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ViewId",
                table: "AppProjects",
                newName: "ProjectViewId");

            migrationBuilder.RenameColumn(
                name: "AppProjectViewId",
                table: "AppProjects",
                newName: "AppUniqueName");
        }
    }
}
