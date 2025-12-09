using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

namespace SistemaGestionDocumentos.API.Utils
{
    /// <summary>
    /// Servicio de validación centralizado para el sistema
    /// Contiene todas las validaciones necesarias
    /// </summary>
    public static class ValidadorDelSistema
    {
        /// <summary>Validar que el correo sea válido</summary>
        public static (bool esValido, string mensaje) ValidarCorreoElectronico(string correo)
        {
            if (string.IsNullOrWhiteSpace(correo))
                return (false, "El correo electrónico es requerido");

            correo = correo.Trim();

            if (correo.Length > 255)
                return (false, "El correo no puede exceder 255 caracteres");

            // Patrón regex para validar correo
            string patronCorreo = @"^[^\s@]+@[^\s@]+\.[^\s@]+$";
            if (!Regex.IsMatch(correo, patronCorreo))
                return (false, "El correo electrónico no tiene un formato válido");

            // Validar que no tenga caracteres especiales peligrosos
            if (correo.Contains("..") || correo.StartsWith(".") || correo.EndsWith("."))
                return (false, "El correo electrónico tiene un formato inválido");

            return (true, "Correo válido");
        }

        /// <summary>Validar que la contraseña sea fuerte</summary>
        public static (bool esValida, string mensaje) ValidarContraseña(string contraseña)
        {
            if (string.IsNullOrWhiteSpace(contraseña))
                return (false, "La contraseña es requerida");

            if (contraseña.Length < 8)
                return (false, "La contraseña debe tener al menos 8 caracteres");

            if (contraseña.Length > 128)
                return (false, "La contraseña no puede exceder 128 caracteres");

            // Validar que tenga al menos una mayúscula
            if (!contraseña.Any(char.IsUpper))
                return (false, "La contraseña debe contener al menos una letra mayúscula");

            // Validar que tenga al menos una minúscula
            if (!contraseña.Any(char.IsLower))
                return (false, "La contraseña debe contener al menos una letra minúscula");

            // Validar que tenga al menos un número
            if (!contraseña.Any(char.IsDigit))
                return (false, "La contraseña debe contener al menos un número");

            // Validar que tenga al menos un carácter especial
            string caracteresEspeciales = "!@#$%^&*()-_=+[]{}|;:,.<>?";
            if (!contraseña.Any(c => caracteresEspeciales.Contains(c)))
                return (false, "La contraseña debe contener al menos un carácter especial (!@#$%^&*)");

            return (true, "Contraseña válida");
        }

        /// <summary>Validar nombre completo</summary>
        public static (bool esValido, string mensaje) ValidarNombreCompleto(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return (false, "El nombre completo es requerido");

            nombre = nombre.Trim();

            if (nombre.Length < 3)
                return (false, "El nombre debe tener al menos 3 caracteres");

            if (nombre.Length > 200)
                return (false, "El nombre no puede exceder 200 caracteres");

            // Validar que solo contenga letras, espacios y algunos caracteres especiales permitidos
            if (!Regex.IsMatch(nombre, @"^[a-záéíóúñA-ZÁÉÍÓÚÑ\s'-]+$"))
                return (false, "El nombre contiene caracteres no permitidos");

            return (true, "Nombre válido");
        }

        /// <summary>Validar rol del usuario</summary>
        public static (bool esValido, string mensaje) ValidarRol(string rol)
        {
            if (string.IsNullOrWhiteSpace(rol))
                return (false, "El rol es requerido");

            string[] rolesValidos = { "Solicitante", "Aprobador", "Admin" };
            
            if (!rolesValidos.Contains(rol, StringComparer.OrdinalIgnoreCase))
                return (false, $"Rol inválido. Los roles permitidos son: {string.Join(", ", rolesValidos)}");

            return (true, "Rol válido");
        }

        /// <summary>Validar título de documento</summary>
        public static (bool esValido, string mensaje) ValidarTituloDocumento(string titulo)
        {
            if (string.IsNullOrWhiteSpace(titulo))
                return (false, "El título del documento es requerido");

            titulo = titulo.Trim();

            if (titulo.Length < 3)
                return (false, "El título debe tener al menos 3 caracteres");

            if (titulo.Length > 500)
                return (false, "El título no puede exceder 500 caracteres");

            return (true, "Título válido");
        }

        /// <summary>Validar descripción de documento</summary>
        public static (bool esValido, string mensaje) ValidarDescripcion(string descripcion)
        {
            if (string.IsNullOrWhiteSpace(descripcion))
                return (false, "La descripción es requerida");

            descripcion = descripcion.Trim();

            if (descripcion.Length < 10)
                return (false, "La descripción debe tener al menos 10 caracteres");

            if (descripcion.Length > 2000)
                return (false, "La descripción no puede exceder 2000 caracteres");

            return (true, "Descripción válida");
        }

        /// <summary>Validar tipo de documento</summary>
        public static (bool esValido, string mensaje) ValidarTipoDocumento(string tipoDocumento)
        {
            if (string.IsNullOrWhiteSpace(tipoDocumento))
                return (false, "El tipo de documento es requerido");

            string[] tiposValidos = { "Propuesta", "Contrato", "Reporte", "Especificación", "Otro" };
            
            if (!tiposValidos.Contains(tipoDocumento, StringComparer.OrdinalIgnoreCase))
                return (false, $"Tipo de documento inválido. Los tipos permitidos son: {string.Join(", ", tiposValidos)}");

            return (true, "Tipo de documento válido");
        }

        /// <summary>Validar comentarios de solicitud/aprobación</summary>
        public static (bool esValido, string mensaje) ValidarComentarios(string comentarios, bool esRequerido = false)
        {
            if (string.IsNullOrWhiteSpace(comentarios))
            {
                if (esRequerido)
                    return (false, "Los comentarios son requeridos");
                return (true, "Comentarios válidos");
            }

            comentarios = comentarios.Trim();

            if (comentarios.Length < 5)
                return (false, "Los comentarios deben tener al menos 5 caracteres");

            if (comentarios.Length > 1000)
                return (false, "Los comentarios no pueden exceder 1000 caracteres");

            return (true, "Comentarios válidos");
        }

        /// <summary>Validar tamaño de archivo</summary>
        public static (bool esValido, string mensaje) ValidarTamañoArchivo(long tamañoBytes)
        {
            const long TAMANIO_MAXIMO_MB = 50;
            const long TAMANIO_MAXIMO_BYTES = TAMANIO_MAXIMO_MB * 1024 * 1024;

            if (tamañoBytes <= 0)
                return (false, "El archivo está vacío");

            if (tamañoBytes > TAMANIO_MAXIMO_BYTES)
                return (false, $"El archivo no puede exceder {TAMANIO_MAXIMO_MB} MB");

            return (true, "Tamaño de archivo válido");
        }

        /// <summary>Validar extensión de archivo</summary>
        public static (bool esValido, string mensaje) ValidarExtensionArchivo(string nombreArchivo)
        {
            if (string.IsNullOrWhiteSpace(nombreArchivo))
                return (false, "El nombre del archivo es requerido");

            // Extensiones permitidas
            string[] extensionesPermitidas = { 
                ".pdf", ".doc", ".docx", ".xls", ".xlsx", 
                ".ppt", ".pptx", ".txt", ".jpg", ".jpeg", 
                ".png", ".gif", ".zip", ".rar" 
            };

            string extension = Path.GetExtension(nombreArchivo).ToLower();

            if (string.IsNullOrEmpty(extension))
                return (false, "El archivo debe tener una extensión válida");

            if (!extensionesPermitidas.Contains(extension))
                return (false, $"Tipo de archivo no permitido. Extensiones válidas: {string.Join(", ", extensionesPermitidas)}");

            return (true, "Extensión válida");
        }

        /// <summary>Sanitizar texto para evitar inyecciones</summary>
        public static string SanitizarTexto(string texto)
        {
            if (string.IsNullOrEmpty(texto))
                return "";

            // Remover caracteres de control
            texto = Regex.Replace(texto, @"[\x00-\x08\x0B\x0C\x0E-\x1F\x7F]", "");

            return texto.Trim();
        }

        /// <summary>Validar que el correo no esté duplicado en la base de datos</summary>
        public static async Task<(bool esValido, string mensaje)> ValidarCorreoUnico(
            string correo, 
            SistemaGestionDocumentos.API.Data.SistemaGestionDocumentosDbContext contexto,
            int? usuarioIdExcluir = null)
        {
            var usuarioExistente = await contexto.UsuariosDelSistema
                .FirstOrDefaultAsync(u => u.CorreoElectronicoDelUsuario == correo.ToLower());

            // Si no existe el usuario, el correo es único
            if (usuarioExistente == null)
                return (true, "Correo único");

            // Si existe pero es el mismo usuario (para actualizaciones), permitir
            if (usuarioIdExcluir.HasValue && usuarioExistente.IdentificadorUsuario == usuarioIdExcluir.Value)
                return (true, "Correo único");

            return (false, "El correo electrónico ya está registrado en el sistema");
        }
    }
}
