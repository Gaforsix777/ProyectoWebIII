using System;

namespace SistemaGestionDocumentos.API.Models
{
    /// <summary>
    /// Entidad que registra todas las acciones del sistema para auditoría
    /// Quién hizo qué, cuándo y dónde
    /// </summary>
    public class AuditoriaAccion
    {
        /// <summary>Identificador único del registro de auditoría</summary>
        public int IdentificadorRegistroAuditoria { get; set; }

        /// <summary>ID del usuario que realizó la acción</summary>
        public int IdentificadorUsuarioQueRealizaAccion { get; set; }

        /// <summary>Descripción de la acción realizada (ej: "Subida de Documento", "Aprobación", etc)</summary>
        public string DescripcionAccionRealizada { get; set; } = "";

        /// <summary>Fecha y hora exacta de la acción</summary>
        public DateTime FechaHoraExactaAccion { get; set; } = DateTime.UtcNow;

        /// <summary>Detalles adicionales de la acción (IP, navegador, etc)</summary>
        public string DetallesAdicionalesAccion { get; set; } = "";

        /// <summary>ID del documento relacionado (si aplica)</summary>
        public int? IdentificadorDocumentoRelacionadoAccion { get; set; }

        /// <summary>Dirección IP desde donde se realizó la acción</summary>
        public string DireccionIPDelDispositivoAccion { get; set; } = "";

        // Relación de navegación
        public Usuario UsuarioQueRealiza { get; set; }
    }
}
