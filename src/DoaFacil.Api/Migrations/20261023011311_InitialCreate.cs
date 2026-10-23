using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DoaFacil.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PontosColeta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Endereco = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Cidade = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Uf = table.Column<string>(type: "TEXT", fixedLength: true, maxLength: 2, nullable: false),
                    Telefone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PontosColeta", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Doacoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NomeDoador = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    Categoria = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Quantidade = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    RecebidaEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AtualizadaEm = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PontoColetaId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Doacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Doacoes_PontosColeta_PontoColetaId",
                        column: x => x.PontoColetaId,
                        principalTable: "PontosColeta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "PontosColeta",
                columns: new[] { "Id", "Ativo", "Cidade", "CriadoEm", "Endereco", "Nome", "Telefone", "Uf" },
                values: new object[,]
                {
                    { 1, true, "São Paulo", new DateTime(2026, 10, 1, 9, 0, 0, 0, DateTimeKind.Utc), "Rua das Flores, 120 - Centro", "ONG Mãos Solidárias", "(11) 3333-1000", "SP" },
                    { 2, true, "Campinas", new DateTime(2026, 10, 2, 14, 30, 0, 0, DateTimeKind.Utc), "Av. Brasil, 2500 - Jardim América", "Paróquia São José", "(19) 3222-4500", "SP" }
                });

            migrationBuilder.InsertData(
                table: "Doacoes",
                columns: new[] { "Id", "AtualizadaEm", "Categoria", "Descricao", "NomeDoador", "PontoColetaId", "Quantidade", "RecebidaEm", "Status" },
                values: new object[,]
                {
                    { 1, null, "Alimento", "Cestas básicas completas", "Maria Oliveira", 1, 10, new DateTime(2026, 10, 5, 10, 0, 0, 0, DateTimeKind.Utc), "Recebida" },
                    { 2, null, "Roupa", "Agasalhos de inverno (adulto)", "João Santos", 2, 25, new DateTime(2026, 10, 6, 16, 45, 0, 0, DateTimeKind.Utc), "EmTriagem" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Doacoes_PontoColetaId",
                table: "Doacoes",
                column: "PontoColetaId");

            migrationBuilder.CreateIndex(
                name: "IX_Doacoes_Status",
                table: "Doacoes",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PontosColeta_Nome_Cidade",
                table: "PontosColeta",
                columns: new[] { "Nome", "Cidade" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Doacoes");

            migrationBuilder.DropTable(
                name: "PontosColeta");
        }
    }
}
