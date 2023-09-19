using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZiraLink.Api.Application.Migrations
{
    /// <inheritdoc />
    public partial class ChangeViewIdColumnInAppProjectEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ViewId",
                table: "AppProjects",
                newName: "ProjectViewId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProjectViewId",
                table: "AppProjects",
                newName: "ViewId");
        }
    }
}
