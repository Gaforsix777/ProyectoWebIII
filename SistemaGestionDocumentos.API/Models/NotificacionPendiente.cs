using System;

namespace SistemaGestionDocumentos.API.Models
{
    /// <summary>
    /// Entidad que almacena notificaciones pendientes de enviar a usuarios
    /// Se envían por correo y se registran en el sistema
    /// </summary>
    public class NotificacionPendiente
    {
        /// <summary>Identificador único de la notificación</summary>
        public int IdentificadorNotificacionPendiente { get; set; }

        /// <summary>ID del usuario que recibirá la notificación</summary>
        public int IdentificadorUsuarioReceptorNotificacion { get; set; }

        /// <summary>ID del documento relacionado (si aplica)</summary>
        public int? IdentificadorDocumentoRelacionadoNotificacion { get; set; }

        /// <summary>Tipo de notificación: "SolicitudAprobacion", "Aprobado", "Rechazado", "NuevaVersion"</summary>
        public string TipoNotificacionEnviar { get; set; } = "";

        /// <summary>Asunto del correo de notificación</summary>
        public string AsuntoNotificacionCorreo { get; set; } = "";

        /// <summary>Cuerpo del mensaje de notificación</summary>
        public string ContenidoMensajeNotificacion { get; set; } = "";

        /// <summary>Indica si ya se leyó la notificación (true = leída, false = no leída)</summary>
        public bool IndicadorNotificacionLeidaONo { get; set; } = false;

        /// <summary>Fecha y hora de creación de la notificación</summary>
        public DateTime FechaCreacionNotificacion { get; set; } = DateTime.UtcNow;

        /// <summary>Fecha y hora de envío de la notificación (null si no se envió aún)</summary>
        public DateTime? FechaEnvioNotificacion { get; set; }

        // Relaciones de navegación
        public Usuario UsuarioReceptor { get; set; }
        public Documento DocumentoRelacionado { get; set; }
    }
}
