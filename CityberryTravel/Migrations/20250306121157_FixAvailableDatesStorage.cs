using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CityberryTravel.Migrations
{
    /// <inheritdoc />
    public partial class FixAvailableDatesStorage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "AvailableDates",
                table: "TravelDestinations",
                type: "text",
                nullable: false,
                oldClrType: typeof(List<DateTime>),
                oldType: "timestamp with time zone[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<List<DateTime>>(
                name: "AvailableDates",
                table: "TravelDestinations",
                type: "timestamp with time zone[]",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
