using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OwaspForge.Web.Data.Migrations;

[DbContext(typeof(ForgeDbContext))]
[Migration("20260907100000_Scoring")]
public partial class Scoring : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(name: "Attempts", table: "Progress", type: "INTEGER", nullable: false, defaultValue: 0);
        migrationBuilder.AddColumn<int>(name: "HintsUsed", table: "Progress", type: "INTEGER", nullable: false, defaultValue: 0);
        migrationBuilder.AddColumn<int>(name: "PenaltyPoints", table: "Progress", type: "INTEGER", nullable: false, defaultValue: 0);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "Attempts", table: "Progress");
        migrationBuilder.DropColumn(name: "HintsUsed", table: "Progress");
        migrationBuilder.DropColumn(name: "PenaltyPoints", table: "Progress");
    }
}
