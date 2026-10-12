using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimeOffAccrual.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCreditAdjustments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CreditAdjustments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AgentUserId = table.Column<int>(type: "INTEGER", nullable: false),
                    AdminUserId = table.Column<int>(type: "INTEGER", nullable: false),
                    PreviousEarnedHours = table.Column<decimal>(type: "TEXT", nullable: false),
                    NewEarnedHours = table.Column<decimal>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditAdjustments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CreditAdjustments_Users_AdminUserId",
                        column: x => x.AdminUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CreditAdjustments_Users_AgentUserId",
                        column: x => x.AgentUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CreditAdjustments_AdminUserId",
                table: "CreditAdjustments",
                column: "AdminUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditAdjustments_AgentUserId",
                table: "CreditAdjustments",
                column: "AgentUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CreditAdjustments");
        }
    }
}
