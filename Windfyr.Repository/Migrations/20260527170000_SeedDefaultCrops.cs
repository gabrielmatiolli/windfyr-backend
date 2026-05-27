using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Windfyr.Repository.Context;

#nullable disable

namespace Windfyr.Repository.Migrations;

[DbContext(typeof(WindfyrDbContext))]
[Migration("20260527170000_SeedDefaultCrops")]
public class SeedDefaultCrops : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            INSERT INTO crops (id, type, created_at, updated_at)
            SELECT 1, 'Milho', UTC_TIMESTAMP(), UTC_TIMESTAMP()
            FROM DUAL
            WHERE NOT EXISTS (SELECT 1 FROM crops WHERE id = 1);
            """);

        migrationBuilder.Sql("""
            INSERT INTO crops (id, type, created_at, updated_at)
            SELECT 2, 'Soja', UTC_TIMESTAMP(), UTC_TIMESTAMP()
            FROM DUAL
            WHERE NOT EXISTS (SELECT 1 FROM crops WHERE id = 2);
            """);

        migrationBuilder.Sql("""
            INSERT INTO crops (id, type, created_at, updated_at)
            SELECT 3, 'Cafe', UTC_TIMESTAMP(), UTC_TIMESTAMP()
            FROM DUAL
            WHERE NOT EXISTS (SELECT 1 FROM crops WHERE id = 3);
            """);

        migrationBuilder.Sql("""
            INSERT INTO crops (id, type, created_at, updated_at)
            SELECT 4, 'Cana-de-acucar', UTC_TIMESTAMP(), UTC_TIMESTAMP()
            FROM DUAL
            WHERE NOT EXISTS (SELECT 1 FROM crops WHERE id = 4);
            """);

        migrationBuilder.Sql("""
            INSERT INTO crops (id, type, created_at, updated_at)
            SELECT 5, 'Algodao', UTC_TIMESTAMP(), UTC_TIMESTAMP()
            FROM DUAL
            WHERE NOT EXISTS (SELECT 1 FROM crops WHERE id = 5);
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DELETE FROM crops WHERE id IN (1, 2, 3, 4, 5);");
    }
}