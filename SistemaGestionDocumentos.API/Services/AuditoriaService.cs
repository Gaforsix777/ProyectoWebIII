using SistemaGestionDocumentos.API.Data;
using SistemaGestionDocumentos.API.Models;

namespace SistemaGestionDocumentos.API.Services
{
    /// <summary>
    /// Servicio para registrar automáticamente acciones en auditoría
    /// </summary>
    public class AuditoriaService
    {
        private readonly SistemaGestionDocumentosDbContext _contextoBD;

        public AuditoriaService(SistemaGestionDocumentosDbContext contextoBD)
        {
            _contextoBD = contextoBD;
        }

        /// <summary>
        /// Registra una acción de auditoría en la base de datos
        /// </summary>
        public async Task RegistrarAccionAuditoria(
            int usuarioId,
            string descripcion,
            string detalles = "",
            int? documentoRelacionadoId = null,
            string direccionIp = "")
        {
            try
            {
                var registroAuditoria = new AuditoriaAccion
                {
                    IdentificadorUsuarioQueRealizaAccion = usuarioId,
                    DescripcionAccionRealizada = descripcion,
                    FechaHoraExactaAccion = DateTime.UtcNow,
                    DetallesAdicionalesAccion = detalles,
                    IdentificadorDocumentoRelacionadoAccion = documentoRelacionadoId,
                    DireccionIPDelDispositivoAccion = direccionIp
                };

                _contextoBD.AuditoriaAccionDelSistema.Add(registroAuditoria);
                await _contextoBD.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log del error pero no lanzo excepción para no romper la operación principal
                Console.WriteLine($"Error registrando auditoría: {ex.Message}");
            }
        }
    }
}
