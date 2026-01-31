using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class Transaction_Refactor_00 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RecurrenceDay",
                table: "Transactions",
                newName: "RecurrenceWeekDay");

            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "Transactions",
                newName: "TotalAmount");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartDate",
                table: "Transactions",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<DateTime>(
                name: "FinishDate",
                table: "Transactions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RecurrenceMonthDay",
                table: "Transactions",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FinishDate",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "RecurrenceMonthDay",
                table: "Transactions");

            migrationBuilder.RenameColumn(
                name: "TotalAmount",
                table: "Transactions",
                newName: "Amount");

            migrationBuilder.RenameColumn(
                name: "RecurrenceWeekDay",
                table: "Transactions",
                newName: "RecurrenceDay");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "StartDate",
                table: "Transactions",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");
        }
    }
}
