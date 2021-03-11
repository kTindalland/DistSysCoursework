using Microsoft.EntityFrameworkCore.Migrations;

namespace DistSysAcw.Migrations
{
    public partial class AddedUsernameToArchive : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "ArchivedLogs",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Username",
                table: "ArchivedLogs");
        }
    }
}
