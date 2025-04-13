using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PublicApi.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class Start : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "user");

            migrationBuilder.CreateTable(
                name: "favorite_currency_rates",
                schema: "user",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    currency = table.Column<string>(type: "text", nullable: false),
                    base_currency = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_favorite_currency_rates", x => x.id);
                    table.CheckConstraint("CK_Currency_NotEqual", "\"currency\" <> \"base_currency\"");
                });

            migrationBuilder.CreateIndex(
                name: "ix_favorite_currency_rates_currency_base_currency",
                schema: "user",
                table: "favorite_currency_rates",
                columns: new[] { "currency", "base_currency" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_favorite_currency_rates_name",
                schema: "user",
                table: "favorite_currency_rates",
                column: "name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "favorite_currency_rates",
                schema: "user");
        }
    }
}
