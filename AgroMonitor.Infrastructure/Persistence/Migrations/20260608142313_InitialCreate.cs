using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgroMonitor.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TB_CAD_SPECIES",
                columns: table => new
                {
                    ID = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    COMMON_NAME = table.Column<string>(type: "NVARCHAR2(80)", maxLength: 80, nullable: false),
                    SCIENTIFIC_NAME = table.Column<string>(type: "NVARCHAR2(120)", maxLength: 120, nullable: true),
                    MIN_HUMIDITY = table.Column<decimal>(type: "DECIMAL(5,2)", precision: 5, scale: 2, nullable: false),
                    MAX_HUMIDITY = table.Column<decimal>(type: "DECIMAL(5,2)", precision: 5, scale: 2, nullable: false),
                    FROST_MIN_TEMP = table.Column<decimal>(type: "DECIMAL(4,1)", precision: 4, scale: 1, nullable: true),
                    WATERING_ML = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_CAD_SPECIES", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "TB_CAD_SLOT",
                columns: table => new
                {
                    ID = table.Column<long>(type: "NUMBER(19)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    SPECIES_ID = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    POSITION = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    PLANTED_AT = table.Column<DateTime>(type: "DATE", nullable: false),
                    STATUS = table.Column<string>(type: "NVARCHAR2(15)", maxLength: 15, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_CAD_SLOT", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TB_CAD_SLOT_TB_CAD_SPECIES_SPECIES_ID",
                        column: x => x.SPECIES_ID,
                        principalTable: "TB_CAD_SPECIES",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TB_CAD_SLOT_SPECIES_ID",
                table: "TB_CAD_SLOT",
                column: "SPECIES_ID");

            migrationBuilder.CreateIndex(
                name: "IX_TB_CAD_SPECIES_COMMON_NAME",
                table: "TB_CAD_SPECIES",
                column: "COMMON_NAME",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TB_CAD_SLOT");

            migrationBuilder.DropTable(
                name: "TB_CAD_SPECIES");
        }
    }
}
