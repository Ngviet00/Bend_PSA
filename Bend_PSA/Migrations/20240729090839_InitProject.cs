using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bend_PSA.Migrations
{
    /// <inheritdoc />
    public partial class InitProject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "data",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    model = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    roll = table.Column<int>(type: "int", nullable: true),
                    result_1 = table.Column<int>(type: "int", nullable: true),
                    result_2 = table.Column<int>(type: "int", nullable: true),
                    timeline = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_data", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "errors",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    data_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    type_error = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_errors", x => x.id);
                    table.ForeignKey(
                        name: "FK_errors_data_data_id",
                        column: x => x.data_id,
                        principalTable: "data",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "images",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    data_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    path_url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_images", x => x.id);
                    table.ForeignKey(
                        name: "FK_images_data_data_id",
                        column: x => x.data_id,
                        principalTable: "data",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_data_timeline",
                table: "data",
                column: "timeline");

            migrationBuilder.CreateIndex(
                name: "IX_errors_data_id",
                table: "errors",
                column: "data_id");

            migrationBuilder.CreateIndex(
                name: "IX_images_data_id",
                table: "images",
                column: "data_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "errors");

            migrationBuilder.DropTable(
                name: "images");

            migrationBuilder.DropTable(
                name: "data");
        }
    }
}
