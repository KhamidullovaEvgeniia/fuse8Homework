using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InternalApi.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class DateId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_currency_rates_exchange_dates_date",
                schema: "cur",
                table: "currency_rates");

            migrationBuilder.DropUniqueConstraint(
                name: "ak_exchange_dates_date",
                schema: "cur",
                table: "exchange_dates");

            migrationBuilder.DropPrimaryKey(
                name: "pk_currency_rates",
                schema: "cur",
                table: "currency_rates");

            migrationBuilder.DropIndex(
                name: "ix_currency_rates_date",
                schema: "cur",
                table: "currency_rates");

            migrationBuilder.DropColumn(
                name: "date",
                schema: "cur",
                table: "currency_rates");

            migrationBuilder.AddColumn<int>(
                name: "date_id",
                schema: "cur",
                table: "currency_rates",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "pk_currency_rates",
                schema: "cur",
                table: "currency_rates",
                columns: new[] { "currency", "date_id" });

            migrationBuilder.CreateIndex(
                name: "ix_currency_rates_date_id",
                schema: "cur",
                table: "currency_rates",
                column: "date_id");

            migrationBuilder.AddForeignKey(
                name: "fk_currency_rates_exchange_dates_date_id",
                schema: "cur",
                table: "currency_rates",
                column: "date_id",
                principalSchema: "cur",
                principalTable: "exchange_dates",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_currency_rates_exchange_dates_date_id",
                schema: "cur",
                table: "currency_rates");

            migrationBuilder.DropPrimaryKey(
                name: "pk_currency_rates",
                schema: "cur",
                table: "currency_rates");

            migrationBuilder.DropIndex(
                name: "ix_currency_rates_date_id",
                schema: "cur",
                table: "currency_rates");

            migrationBuilder.DropColumn(
                name: "date_id",
                schema: "cur",
                table: "currency_rates");

            migrationBuilder.AddColumn<DateTime>(
                name: "date",
                schema: "cur",
                table: "currency_rates",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddUniqueConstraint(
                name: "ak_exchange_dates_date",
                schema: "cur",
                table: "exchange_dates",
                column: "date");

            migrationBuilder.AddPrimaryKey(
                name: "pk_currency_rates",
                schema: "cur",
                table: "currency_rates",
                columns: new[] { "currency", "date" });

            migrationBuilder.CreateIndex(
                name: "ix_currency_rates_date",
                schema: "cur",
                table: "currency_rates",
                column: "date");

            migrationBuilder.AddForeignKey(
                name: "fk_currency_rates_exchange_dates_date",
                schema: "cur",
                table: "currency_rates",
                column: "date",
                principalSchema: "cur",
                principalTable: "exchange_dates",
                principalColumn: "date",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
