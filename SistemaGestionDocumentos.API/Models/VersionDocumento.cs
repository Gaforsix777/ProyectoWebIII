using System;

namespace SistemaGestionDocumentos.API.Models
{
    /// <summary>
    /// Entidad que registra las diferentes versiones de un documento
    /// Permite el versionamiento y auditoría de cambios
    /// </summary>
    public class VersionDocumento
    {
        /// <summary>Identificador único de la versión</summary>
        public int IdentificadorVersionDocumento { get; set; }

        /// <summary>ID del documento al que pertenece esta versión</summary>
        public int IdentificadorDocumentoPerteneceVersion { get; set; }

        /// <summary>Nombre original del archivo de esta versión</summary>
        public string NombreArchivoDelVersionDocumento { get; set; } = "";

        /// <summary>Ruta física del archivo en esta versión</summary>
        public string RutaFisicaDelArchivoVersion { get; set; } = "";

        /// <summary>Número secuencial de la versión (1, 2, 3...)</summary>
        public int NumeroSecuencialDelVersionDocumento { get; set; }

        /// <summary>Fecha y hora de creación de esta versión</summary>
        public DateTime FechaCreacionDelVersionDocumento { get; set; } = DateTime.UtcNow;

        /// <summary>Comentarios o cambios realizados en esta versión</summary>
        public string ComentariosDelCambioDeVersion { get; set; } = "";

        // Relación de navegación
        public Documento DocumentoAlQuePerteneceLaVersion { get; set; }
    }
}
