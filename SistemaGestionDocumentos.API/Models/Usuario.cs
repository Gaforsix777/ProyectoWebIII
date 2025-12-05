using System;
using System.Collections.Generic;

namespace SistemaGestionDocumentos.API.Models
{
    /// <summary>
    /// Entidad que representa un usuario del sistema
    /// Puede ser: Solicitante, Aprobador o Admin
    /// </summary>
    public class Usuario
    {
        /// <summary>Identificador único del usuario</summary>
        public int IdentificadorUsuario { get; set; }

        /// <summary>Correo electrónico único del usuario (login)</summary>
        public string CorreoElectronicoDelUsuario { get; set; } = "";

        /// <summary>Contraseña hasheada del usuario (NO guardar en texto plano)</summary>
        public string ContraseñaHashDelUsuario { get; set; } = "";

        /// <summary>Nombre completo del usuario</summary>
        public string NombreCompletoDelUsuario { get; set; } = "";

        /// <summary>Rol del usuario: "Solicitante", "Aprobador", "Admin"</summary>
        public string RolDelUsuario { get; set; } = "Solicitante";

        /// <summary>Fecha y hora en que se creó el usuario</summary>
        public DateTime FechaCreacionDelUsuario { get; set; } = DateTime.UtcNow;

        /// <summary>Indica si el usuario está activo o desactivado</summary>
        public bool IndicadorUsuarioActivo { get; set; } = true;

        // Relaciones de navegación
        public ICollection<Documento> DocumentosDelUsuario { get; set; } = new List<Documento>();
        public ICollection<WorkflowAprobacion> AprobacionesDelUsuario { get; set; } = new List<WorkflowAprobacion>();
        public ICollection<AuditoriaAccion> AccionesAuditoriasDelUsuario { get; set; } = new List<AuditoriaAccion>();
        public ICollection<NotificacionPendiente> NotificacionesDelUsuario { get; set; } = new List<NotificacionPendiente>();
    }
}
