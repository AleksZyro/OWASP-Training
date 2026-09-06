using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OwaspForge.Web.Data.Migrations;

[DbContext(typeof(ForgeDbContext))]
[Migration(ForgeDatabaseInitializer.InitialMigrationId)]
public partial class InitialProgress : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Progress",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                ChallengeId = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                IsCompleted = table.Column<bool>(type: "INTEGER", nullable: false),
                CompletedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
            },
            constraints: table => table.PrimaryKey("PK_Progress", x => x.Id));

        migrationBuilder.CreateIndex(
            name: "IX_Progress_ChallengeId",
            table: "Progress",
            column: "ChallengeId",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable(name: "Progress");
}
