using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IPO.Correspondence.Data.Migrations
{
    public partial class Adding_Error_Column_Message_Table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Errors",
                table: "Messages",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Errors",
                table: "Messages");
        }
    }
}
