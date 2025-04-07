using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace InternalApi.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "cur");

            migrationBuilder.CreateTable(
                name: "exchange_dates",
                schema: "cur",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_exchange_dates", x => x.id);
                    table.UniqueConstraint("ak_exchange_dates_date", x => x.date);
                });

            migrationBuilder.CreateTable(
                name: "currency_rates",
                schema: "cur",
                columns: table => new
                {
                    currency = table.Column<int>(type: "integer", nullable: false),
                    date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    value = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_currency_rates", x => new { x.currency, x.date });
                    table.ForeignKey(
                        name: "fk_currency_rates_exchange_dates_date",
                        column: x => x.date,
                        principalSchema: "cur",
                        principalTable: "exchange_dates",
                        principalColumn: "date",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_currency_rates_date",
                schema: "cur",
                table: "currency_rates",
                column: "date");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "currency_rates",
                schema: "cur");

            migrationBuilder.DropTable(
                name: "exchange_dates",
                schema: "cur");
        }
    }
}
