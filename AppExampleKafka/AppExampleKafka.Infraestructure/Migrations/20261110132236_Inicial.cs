using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppExampleKafka.Infraestructure.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Cadastro_TipoPessoa = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    Cadastro_Documento = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: true),
                    Cadastro_Telefone = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: true),
                    Cadastro_Endereco_Logradouro = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: true),
                    Cadastro_Endereco_Numero = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Cadastro_Endereco_Bairro = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Cadastro_Endereco_Cidade = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Cadastro_Endereco_Uf = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    Cadastro_Endereco_Cep = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    Cadastro_Endereco_Complemento = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Cadastro_PessoaFisica_DataNascimento = table.Column<DateOnly>(type: "date", nullable: true),
                    Cadastro_PessoaFisica_Nacionalidade = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    Cadastro_PessoaFisica_EstadoCivil = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Cadastro_PessoaFisica_Profissao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Cadastro_PessoaFisica_RendaMensal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Cadastro_PessoaJuridica_RazaoSocial = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: true),
                    Cadastro_PessoaJuridica_NomeFantasia = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: true),
                    Cadastro_PessoaJuridica_DataConstituicao = table.Column<DateOnly>(type: "date", nullable: true),
                    Cadastro_PessoaJuridica_NaturezaJuridica = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Cadastro_PessoaJuridica_Cnae = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    Cadastro_PessoaJuridica_FaturamentoMensal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Cadastro_PessoaJuridica_RepresentanteLegal_Nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    Cadastro_PessoaJuridica_RepresentanteLegal_Cpf = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: true),
                    Cadastro_PessoaJuridica_RepresentanteLegal_DataNascimento = table.Column<DateOnly>(type: "date", nullable: true),
                    Nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Email = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "contas_bancarias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    Agencia = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    Numero = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    DigitoVerificador = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    TipoConta = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Saldo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Moeda = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Ativa = table.Column<bool>(type: "boolean", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contas_bancarias", x => x.Id);
                    table.CheckConstraint("ck_conta_saldo", "\"Saldo\" >= 0");
                    table.ForeignKey(
                        name: "FK_contas_bancarias_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RevokedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReplacedByTokenHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refresh_tokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_refresh_tokens_usuarios_UserId",
                        column: x => x.UserId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "transacoes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContaOrigemId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContaDestinoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Moeda = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    ChaveIdempotencia = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transacoes", x => x.Id);
                    table.CheckConstraint("ck_transacao_valor", "\"Valor\" > 0 AND \"ContaOrigemId\" <> \"ContaDestinoId\"");
                    table.ForeignKey(
                        name: "FK_transacoes_contas_bancarias_ContaDestinoId",
                        column: x => x.ContaDestinoId,
                        principalTable: "contas_bancarias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_transacoes_contas_bancarias_ContaOrigemId",
                        column: x => x.ContaOrigemId,
                        principalTable: "contas_bancarias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_transacoes_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "outbox_mensagens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TransacaoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Payload = table.Column<string>(type: "text", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PublicadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbox_mensagens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_outbox_mensagens_transacoes_TransacaoId",
                        column: x => x.TransacaoId,
                        principalTable: "transacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_contas_bancarias_Numero",
                table: "contas_bancarias",
                column: "Numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_contas_bancarias_UsuarioId",
                table: "contas_bancarias",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_outbox_mensagens_PublicadoEm_CriadoEm",
                table: "outbox_mensagens",
                columns: new[] { "PublicadoEm", "CriadoEm" });

            migrationBuilder.CreateIndex(
                name: "IX_outbox_mensagens_TransacaoId",
                table: "outbox_mensagens",
                column: "TransacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_TokenHash",
                table: "refresh_tokens",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_UserId",
                table: "refresh_tokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_transacoes_ContaDestinoId",
                table: "transacoes",
                column: "ContaDestinoId");

            migrationBuilder.CreateIndex(
                name: "IX_transacoes_ContaOrigemId",
                table: "transacoes",
                column: "ContaOrigemId");

            migrationBuilder.CreateIndex(
                name: "IX_transacoes_UsuarioId_ChaveIdempotencia",
                table: "transacoes",
                columns: new[] { "UsuarioId", "ChaveIdempotencia" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_Cadastro_Documento",
                table: "usuarios",
                column: "Cadastro_Documento",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_Email",
                table: "usuarios",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "outbox_mensagens");

            migrationBuilder.DropTable(
                name: "refresh_tokens");

            migrationBuilder.DropTable(
                name: "transacoes");

            migrationBuilder.DropTable(
                name: "contas_bancarias");

            migrationBuilder.DropTable(
                name: "usuarios");
        }
    }
}
