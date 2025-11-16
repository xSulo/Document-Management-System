using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dms.Dal.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentSummary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Summary",
                table: "Documents",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Summary",
                table: "Documents");
        }
    }
}
