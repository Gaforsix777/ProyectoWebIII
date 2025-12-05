using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaGestionDocumentos.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UsuariosDelSistema",
                columns: table => new
                {
                    IdentificadorUsuario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CorreoElectronicoDelUsuario = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContraseñaHashDelUsuario = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NombreCompletoDelUsuario = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RolDelUsuario = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaCreacionDelUsuario = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IndicadorUsuarioActivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuariosDelSistema", x => x.IdentificadorUsuario);
                });

            migrationBuilder.CreateTable(
                name: "AuditoriaAccionDelSistema",
                columns: table => new
                {
                    IdentificadorRegistroAuditoria = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdentificadorUsuarioQueRealizaAccion = table.Column<int>(type: "int", nullable: false),
                    DescripcionAccionRealizada = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaHoraExactaAccion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DetallesAdicionalesAccion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdentificadorDocumentoRelacionadoAccion = table.Column<int>(type: "int", nullable: true),
                    DireccionIPDelDispositivoAccion = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditoriaAccionDelSistema", x => x.IdentificadorRegistroAuditoria);
                    table.ForeignKey(
                        name: "FK_AuditoriaAccionDelSistema_UsuariosDelSistema_IdentificadorUsuarioQueRealizaAccion",
                        column: x => x.IdentificadorUsuarioQueRealizaAccion,
                        principalTable: "UsuariosDelSistema",
                        principalColumn: "IdentificadorUsuario");
                });

            migrationBuilder.CreateTable(
                name: "DocumentosDelSistema",
                columns: table => new
                {
                    IdentificadorDocumento = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreDescriptivoDelDocumento = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RutaFisicaDelArchivoDocumento = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NumeroVersionActualDelDocumento = table.Column<int>(type: "int", nullable: false),
                    IdentificadorUsuarioPropietarioDelDocumento = table.Column<int>(type: "int", nullable: false),
                    FechaSubidaDelDocumento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EstadoActualDelDocumento = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DescripcionAdicionalesDelDocumento = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaUltimaModificacionDelDocumento = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentosDelSistema", x => x.IdentificadorDocumento);
                    table.ForeignKey(
                        name: "FK_DocumentosDelSistema_UsuariosDelSistema_IdentificadorUsuarioPropietarioDelDocumento",
                        column: x => x.IdentificadorUsuarioPropietarioDelDocumento,
                        principalTable: "UsuariosDelSistema",
                        principalColumn: "IdentificadorUsuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NotificacionesPendientesDelSistema",
                columns: table => new
                {
                    IdentificadorNotificacionPendiente = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdentificadorUsuarioReceptorNotificacion = table.Column<int>(type: "int", nullable: false),
                    IdentificadorDocumentoRelacionadoNotificacion = table.Column<int>(type: "int", nullable: true),
                    TipoNotificacionEnviar = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AsuntoNotificacionCorreo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContenidoMensajeNotificacion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IndicadorNotificacionLeidaONo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacionNotificacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaEnvioNotificacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificacionesPendientesDelSistema", x => x.IdentificadorNotificacionPendiente);
                    table.ForeignKey(
                        name: "FK_NotificacionesPendientesDelSistema_DocumentosDelSistema_IdentificadorDocumentoRelacionadoNotificacion",
                        column: x => x.IdentificadorDocumentoRelacionadoNotificacion,
                        principalTable: "DocumentosDelSistema",
                        principalColumn: "IdentificadorDocumento",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_NotificacionesPendientesDelSistema_UsuariosDelSistema_IdentificadorUsuarioReceptorNotificacion",
                        column: x => x.IdentificadorUsuarioReceptorNotificacion,
                        principalTable: "UsuariosDelSistema",
                        principalColumn: "IdentificadorUsuario");
                });

            migrationBuilder.CreateTable(
                name: "VersionesDelSistema",
                columns: table => new
                {
                    IdentificadorVersionDocumento = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdentificadorDocumentoPerteneceVersion = table.Column<int>(type: "int", nullable: false),
                    NombreArchivoDelVersionDocumento = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RutaFisicaDelArchivoVersion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NumeroSecuencialDelVersionDocumento = table.Column<int>(type: "int", nullable: false),
                    FechaCreacionDelVersionDocumento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ComentariosDelCambioDeVersion = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VersionesDelSistema", x => x.IdentificadorVersionDocumento);
                    table.ForeignKey(
                        name: "FK_VersionesDelSistema_DocumentosDelSistema_IdentificadorDocumentoPerteneceVersion",
                        column: x => x.IdentificadorDocumentoPerteneceVersion,
                        principalTable: "DocumentosDelSistema",
                        principalColumn: "IdentificadorDocumento",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowAprobacionDelSistema",
                columns: table => new
                {
                    IdentificadorRegistroAprobacion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdentificadorDocumentoAprobar = table.Column<int>(type: "int", nullable: false),
                    EstadoActualDelFlujoAprobacion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdentificadorUsuarioAprobadorDelDocumento = table.Column<int>(type: "int", nullable: false),
                    FechaAccionAprobacionDelDocumento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ComentariosDelAprobadorDelDocumento = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NivelPrioridadDelDocumento = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowAprobacionDelSistema", x => x.IdentificadorRegistroAprobacion);
                    table.ForeignKey(
                        name: "FK_WorkflowAprobacionDelSistema_DocumentosDelSistema_IdentificadorDocumentoAprobar",
                        column: x => x.IdentificadorDocumentoAprobar,
                        principalTable: "DocumentosDelSistema",
                        principalColumn: "IdentificadorDocumento",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkflowAprobacionDelSistema_UsuariosDelSistema_IdentificadorUsuarioAprobadorDelDocumento",
                        column: x => x.IdentificadorUsuarioAprobadorDelDocumento,
                        principalTable: "UsuariosDelSistema",
                        principalColumn: "IdentificadorUsuario");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditoriaAccionDelSistema_IdentificadorUsuarioQueRealizaAccion",
                table: "AuditoriaAccionDelSistema",
                column: "IdentificadorUsuarioQueRealizaAccion");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosDelSistema_IdentificadorUsuarioPropietarioDelDocumento",
                table: "DocumentosDelSistema",
                column: "IdentificadorUsuarioPropietarioDelDocumento");

            migrationBuilder.CreateIndex(
                name: "IX_NotificacionesPendientesDelSistema_IdentificadorDocumentoRelacionadoNotificacion",
                table: "NotificacionesPendientesDelSistema",
                column: "IdentificadorDocumentoRelacionadoNotificacion");

            migrationBuilder.CreateIndex(
                name: "IX_NotificacionesPendientesDelSistema_IdentificadorUsuarioReceptorNotificacion",
                table: "NotificacionesPendientesDelSistema",
                column: "IdentificadorUsuarioReceptorNotificacion");

            migrationBuilder.CreateIndex(
                name: "IX_VersionesDelSistema_IdentificadorDocumentoPerteneceVersion",
                table: "VersionesDelSistema",
                column: "IdentificadorDocumentoPerteneceVersion");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowAprobacionDelSistema_IdentificadorDocumentoAprobar",
                table: "WorkflowAprobacionDelSistema",
                column: "IdentificadorDocumentoAprobar");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowAprobacionDelSistema_IdentificadorUsuarioAprobadorDelDocumento",
                table: "WorkflowAprobacionDelSistema",
                column: "IdentificadorUsuarioAprobadorDelDocumento");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditoriaAccionDelSistema");

            migrationBuilder.DropTable(
                name: "NotificacionesPendientesDelSistema");

            migrationBuilder.DropTable(
                name: "VersionesDelSistema");

            migrationBuilder.DropTable(
                name: "WorkflowAprobacionDelSistema");

            migrationBuilder.DropTable(
                name: "DocumentosDelSistema");

            migrationBuilder.DropTable(
                name: "UsuariosDelSistema");
        }
    }
}
