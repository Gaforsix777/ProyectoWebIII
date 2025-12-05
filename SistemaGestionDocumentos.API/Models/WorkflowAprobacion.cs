using System;

namespace SistemaGestionDocumentos.API.Models
{
    /// <summary>
    /// Entidad que registra el proceso de aprobación de un documento
    /// Un documento puede tener múltiples registros de aprobación
    /// </summary>
    public class WorkflowAprobacion
    {
        /// <summary>Identificador único del registro de aprobación</summary>
        public int IdentificadorRegistroAprobacion { get; set; }

        /// <summary>ID del documento a aprobar</summary>
        public int IdentificadorDocumentoAprobar { get; set; }

        /// <summary>Estado actual del flujo: "Solicitado", "EnRevision", "Aprobado", "Rechazado"</summary>
        public string EstadoActualDelFlujoAprobacion { get; set; } = "Solicitado";

        /// <summary>ID del usuario que aprueba o rechaza</summary>
        public int IdentificadorUsuarioAprobadorDelDocumento { get; set; }

        /// <summary>Fecha y hora de la acción de aprobación/rechazo</summary>
        public DateTime FechaAccionAprobacionDelDocumento { get; set; } = DateTime.UtcNow;

        /// <summary>Comentarios del aprobador (por qué aprobó/rechazó)</summary>
        public string ComentariosDelAprobadorDelDocumento { get; set; } = "";

        /// <summary>Prioridad del documento: "Baja", "Media", "Alta", "Urgente"</summary>
        public string NivelPrioridadDelDocumento { get; set; } = "Media";

        // Relaciones de navegación
        public Documento DocumentoEnAprobacion { get; set; }
        public Usuario UsuarioQueAprueba { get; set; }
    }
}
