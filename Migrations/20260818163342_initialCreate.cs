using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CosturaProducao.Migrations
{
    /// <inheritdoc />
    public partial class initialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Phone = table.Column<string>(type: "TEXT", nullable: true),
                    Address = table.Column<string>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Active = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Phone = table.Column<string>(type: "TEXT", nullable: true),
                    Address = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Active = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PieceModels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    TemplateImagePath = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Active = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PieceModels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Seamstresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Phone = table.Column<string>(type: "TEXT", nullable: true),
                    Address = table.Column<string>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Active = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seamstresses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServiceProcesses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    DefaultPricePerPiece = table.Column<decimal>(type: "TEXT", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Active = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceProcesses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PieceSizes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    PieceModelId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PieceSizes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PieceSizes_PieceModels_PieceModelId",
                        column: x => x.PieceModelId,
                        principalTable: "PieceModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PieceVariants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PieceModelId = table.Column<int>(type: "INTEGER", nullable: false),
                    Cor = table.Column<string>(type: "TEXT", nullable: false),
                    Tamanho = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Active = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PieceVariants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PieceVariants_PieceModels_PieceModelId",
                        column: x => x.PieceModelId,
                        principalTable: "PieceModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PieceServices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PieceModelId = table.Column<int>(type: "INTEGER", nullable: false),
                    ServiceProcessId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PieceServices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PieceServices_PieceModels_PieceModelId",
                        column: x => x.PieceModelId,
                        principalTable: "PieceModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PieceServices_ServiceProcesses_ServiceProcessId",
                        column: x => x.ServiceProcessId,
                        principalTable: "ServiceProcesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Productions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ClientId = table.Column<int>(type: "INTEGER", nullable: false),
                    PieceModelId = table.Column<int>(type: "INTEGER", nullable: false),
                    PieceVariantId = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalQuantity = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductionDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Active = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Productions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Productions_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Productions_PieceModels_PieceModelId",
                        column: x => x.PieceModelId,
                        principalTable: "PieceModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Productions_PieceVariants_PieceVariantId",
                        column: x => x.PieceVariantId,
                        principalTable: "PieceVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductionProcesses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProductionId = table.Column<int>(type: "INTEGER", nullable: false),
                    ServiceProcessId = table.Column<int>(type: "INTEGER", nullable: false),
                    PricePerPiece = table.Column<decimal>(type: "TEXT", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Active = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionProcesses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionProcesses_Productions_ProductionId",
                        column: x => x.ProductionId,
                        principalTable: "Productions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductionProcesses_ServiceProcesses_ServiceProcessId",
                        column: x => x.ServiceProcessId,
                        principalTable: "ServiceProcesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Assignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProductionProcessId = table.Column<int>(type: "INTEGER", nullable: false),
                    SeamstressId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlannedQuantity = table.Column<int>(type: "INTEGER", nullable: false),
                    ProducedQuantity = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    PricePerPiece = table.Column<decimal>(type: "TEXT", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Active = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assignments_ProductionProcesses_ProductionProcessId",
                        column: x => x.ProductionProcessId,
                        principalTable: "ProductionProcesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Assignments_Seamstresses_SeamstressId",
                        column: x => x.SeamstressId,
                        principalTable: "Seamstresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_ProductionProcessId",
                table: "Assignments",
                column: "ProductionProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_SeamstressId",
                table: "Assignments",
                column: "SeamstressId");

            migrationBuilder.CreateIndex(
                name: "IX_PieceServices_PieceModelId_ServiceProcessId",
                table: "PieceServices",
                columns: new[] { "PieceModelId", "ServiceProcessId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PieceServices_ServiceProcessId",
                table: "PieceServices",
                column: "ServiceProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_PieceSizes_PieceModelId",
                table: "PieceSizes",
                column: "PieceModelId");

            migrationBuilder.CreateIndex(
                name: "IX_PieceVariants_PieceModelId_Cor_Tamanho",
                table: "PieceVariants",
                columns: new[] { "PieceModelId", "Cor", "Tamanho" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionProcesses_ProductionId",
                table: "ProductionProcesses",
                column: "ProductionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionProcesses_ServiceProcessId",
                table: "ProductionProcesses",
                column: "ServiceProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_Productions_ClientId",
                table: "Productions",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Productions_PieceModelId",
                table: "Productions",
                column: "PieceModelId");

            migrationBuilder.CreateIndex(
                name: "IX_Productions_PieceVariantId",
                table: "Productions",
                column: "PieceVariantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Assignments");

            migrationBuilder.DropTable(
                name: "Companies");

            migrationBuilder.DropTable(
                name: "PieceServices");

            migrationBuilder.DropTable(
                name: "PieceSizes");

            migrationBuilder.DropTable(
                name: "ProductionProcesses");

            migrationBuilder.DropTable(
                name: "Seamstresses");

            migrationBuilder.DropTable(
                name: "Productions");

            migrationBuilder.DropTable(
                name: "ServiceProcesses");

            migrationBuilder.DropTable(
                name: "Clients");

            migrationBuilder.DropTable(
                name: "PieceVariants");

            migrationBuilder.DropTable(
                name: "PieceModels");
        }
    }
}
