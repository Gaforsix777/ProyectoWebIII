using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaGestionDocumentos.API.Data;
using SistemaGestionDocumentos.API.Models;
using SistemaGestionDocumentos.API.Services;
using SistemaGestionDocumentos.API.Utils;

namespace SistemaGestionDocumentos.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentosController : ControllerBase
    {
        private readonly SistemaGestionDocumentosDbContext _contextoBD;
        private readonly ILogger<DocumentosController> _registradorDeLog;
        private readonly AuditoriaService _servicioAuditoria;

        public DocumentosController(SistemaGestionDocumentosDbContext contextoBD, ILogger<DocumentosController> registrador, AuditoriaService servicioAuditoria)
        {
            _contextoBD = contextoBD;
            _registradorDeLog = registrador;
            _servicioAuditoria = servicioAuditoria;
        }

     /// <summary>
/// Obtener todos los documentos del usuario autenticado
/// GET: api/documentos
/// </summary>
[HttpGet]
public async Task<ActionResult<IEnumerable<object>>> ObtenerTodosLosDocumentos()
{
    try
    {
        // Obtener el ID del usuario desde el header personalizado
        if (!Request.Headers.TryGetValue("X-Usuario-Id", out var usuarioIdHeader) || 
            !int.TryParse(usuarioIdHeader.ToString(), out int usuarioId))
        {
            return BadRequest(new { mensaje = "Usuario no identificado", exitoso = false });
        }

        // Obtener solo los documentos del usuario autenticado
        var documentosDelUsuario = await _contextoBD.DocumentosDelSistema
            .Where(d => d.IdentificadorUsuarioPropietarioDelDocumento == usuarioId)
            .Include(d => d.UsuarioPropietarioDelDocumento)
            .Select(d => new
            {
                d.IdentificadorDocumento,
                d.NombreDescriptivoDelDocumento,
                d.NumeroVersionActualDelDocumento,
                d.EstadoActualDelDocumento,
                d.FechaSubidaDelDocumento,
                d.FechaUltimaModificacionDelDocumento,
                d.DescripcionAdicionalesDelDocumento,
                UsuarioPropietario = d.UsuarioPropietarioDelDocumento.NombreCompletoDelUsuario
            })
            .OrderByDescending(d => d.FechaSubidaDelDocumento)
            .ToListAsync();

        return Ok(documentosDelUsuario);
    }
    catch (Exception excepcion)
    {
        _registradorDeLog.LogError($"Error obteniendo documentos: {excepcion.Message}");
        return StatusCode(500, new { mensaje = "Error interno del servidor", exitoso = false });
    }
}

        /// <summary>
        /// Obtener documento por ID
        /// GET: api/documentos/{id}
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> ObtenerDocumentoPorId(int id)
        {
            try
            {
                var documentoEncontrado = await _contextoBD.DocumentosDelSistema
                    .Include(d => d.UsuarioPropietarioDelDocumento)
                    .Include(d => d.VersionesDelDocumento)
                    .FirstOrDefaultAsync(d => d.IdentificadorDocumento == id);

                if (documentoEncontrado == null)
                    return NotFound(new { mensaje = "Documento no encontrado", exitoso = false });

                return Ok(new
                {
                    documentoEncontrado.IdentificadorDocumento,
                    documentoEncontrado.NombreDescriptivoDelDocumento,
                    documentoEncontrado.NumeroVersionActualDelDocumento,
                    documentoEncontrado.EstadoActualDelDocumento,
                    documentoEncontrado.FechaSubidaDelDocumento,
                    documentoEncontrado.DescripcionAdicionalesDelDocumento,
                    UsuarioPropietario = new
                    {
                        documentoEncontrado.UsuarioPropietarioDelDocumento.IdentificadorUsuario,
                        documentoEncontrado.UsuarioPropietarioDelDocumento.NombreCompletoDelUsuario,
                        documentoEncontrado.UsuarioPropietarioDelDocumento.CorreoElectronicoDelUsuario
                    },
                    Versiones = documentoEncontrado.VersionesDelDocumento.Select(v => new
                    {
                        v.IdentificadorVersionDocumento,
                        v.NumeroSecuencialDelVersionDocumento,
                        v.FechaCreacionDelVersionDocumento,
                        v.ComentariosDelCambioDeVersion
                    })
                });
            }
            catch (Exception excepcion)
            {
                _registradorDeLog.LogError($"Error obteniendo documento: {excepcion.Message}");
                return StatusCode(500, new { mensaje = "Error interno del servidor", exitoso = false });
            }
        }

        /// <summary>
        /// Obtener documentos por usuario
        /// GET: api/documentos/por-usuario/{usuarioId}
        /// </summary>
        [HttpGet("por-usuario/{usuarioId}")]
        public async Task<ActionResult<IEnumerable<object>>> ObtenerDocumentosPorUsuario(int usuarioId)
        {
            try
            {
                var documentosDelUsuario = await _contextoBD.DocumentosDelSistema
                    .Where(d => d.IdentificadorUsuarioPropietarioDelDocumento == usuarioId)
                    .Select(d => new
                    {
                        d.IdentificadorDocumento,
                        d.NombreDescriptivoDelDocumento,
                        d.NumeroVersionActualDelDocumento,
                        d.EstadoActualDelDocumento,
                        d.FechaSubidaDelDocumento,
                        d.DescripcionAdicionalesDelDocumento
                    })
                    .OrderByDescending(d => d.FechaSubidaDelDocumento)
                    .ToListAsync();

                return Ok(documentosDelUsuario);
            }
            catch (Exception excepcion)
            {
                _registradorDeLog.LogError($"Error obteniendo documentos por usuario: {excepcion.Message}");
                return StatusCode(500, new { mensaje = "Error interno del servidor", exitoso = false });
            }
        }

        /// <summary>
        /// Crear nuevo documento
        /// POST: api/documentos/crear
        /// </summary>
        [HttpPost("crear")]
        public async Task<ActionResult<object>> CrearNuevoDocumento(
            [FromBody] ModeloCrearDocumento datosPeticion)
        {
            try
            {
                // Validar título del documento
                var (tituloValido, mensajeTitulo) = ValidadorDelSistema.ValidarTituloDocumento(
                    datosPeticion.NombreDescriptivoDelDocumento);
                if (!tituloValido)
                    return BadRequest(new { mensaje = mensajeTitulo, exitoso = false });

                // Validar descripción
                var (descripcionValida, mensajeDescripcion) = ValidadorDelSistema.ValidarDescripcion(
                    datosPeticion.DescripcionAdicionalesDelDocumento ?? "");
                if (!descripcionValida)
                    return BadRequest(new { mensaje = mensajeDescripcion, exitoso = false });

                // Validar tipo de documento si se proporciona
                if (!string.IsNullOrWhiteSpace(datosPeticion.TipoDocumento))
                {
                    var (tipoValido, mensajeTipo) = ValidadorDelSistema.ValidarTipoDocumento(
                        datosPeticion.TipoDocumento);
                    if (!tipoValido)
                        return BadRequest(new { mensaje = mensajeTipo, exitoso = false });
                }

                // Validar que el usuario exista y esté activo
                var usuarioExiste = await _contextoBD.UsuariosDelSistema
                    .FirstOrDefaultAsync(u => u.IdentificadorUsuario == datosPeticion.IdentificadorUsuarioPropietarioDelDocumento);

                if (usuarioExiste == null)
                    return NotFound(new { mensaje = "Usuario propietario no encontrado", exitoso = false });

                if (!usuarioExiste.IndicadorUsuarioActivo)
                    return BadRequest(new { mensaje = "El usuario propietario está desactivado", exitoso = false });

                // Sanitizar datos
                string tituloLimpio = ValidadorDelSistema.SanitizarTexto(datosPeticion.NombreDescriptivoDelDocumento);
                string descripcionLimpia = ValidadorDelSistema.SanitizarTexto(datosPeticion.DescripcionAdicionalesDelDocumento ?? "");

                var nuevoDocumento = new Documento
                {
                    NombreDescriptivoDelDocumento = tituloLimpio,
                    RutaFisicaDelArchivoDocumento = datosPeticion.RutaFisicaDelArchivoDocumento ?? "",
                    DescripcionAdicionalesDelDocumento = descripcionLimpia,
                    IdentificadorUsuarioPropietarioDelDocumento = datosPeticion.IdentificadorUsuarioPropietarioDelDocumento,
                    NumeroVersionActualDelDocumento = 1,
                    FechaSubidaDelDocumento = DateTime.UtcNow,
                    EstadoActualDelDocumento = "Pendiente",
                    FechaUltimaModificacionDelDocumento = DateTime.UtcNow
                };

                _contextoBD.DocumentosDelSistema.Add(nuevoDocumento);
                await _contextoBD.SaveChangesAsync();

                // Registrar en auditoría
                await _servicioAuditoria.RegistrarAccionAuditoria(
                    datosPeticion.IdentificadorUsuarioPropietarioDelDocumento,
                    "Nuevo documento creado",
                    $"Título: {tituloLimpio} | Tipo: {datosPeticion.TipoDocumento ?? "No especificado"}",
                    nuevoDocumento.IdentificadorDocumento,
                    HttpContext.Connection.RemoteIpAddress?.ToString() ?? ""
                );

                _registradorDeLog.LogInformation($"Nuevo documento creado: ID {nuevoDocumento.IdentificadorDocumento}, Título: {tituloLimpio}");

                return CreatedAtAction(nameof(ObtenerDocumentoPorId), new { id = nuevoDocumento.IdentificadorDocumento },
                    new {
                        mensaje = "Documento creado exitosamente",
                        documentoId = nuevoDocumento.IdentificadorDocumento,
                        titulo = tituloLimpio,
                        estado = "Pendiente",
                        exitoso = true
                    });
            }
            catch (Exception excepcion)
            {
                _registradorDeLog.LogError($"Error creando documento: {excepcion.Message}");
                return StatusCode(500, new { mensaje = "Error interno del servidor", exitoso = false });
            }
        }

        /// <summary>
        /// Subir nuevo documento con archivo
        /// POST: api/documentos/subir
        /// </summary>
        [HttpPost("subir")]
        public async Task<ActionResult<object>> SubirDocumento([FromForm] SubirDocumentoRequest request)
        {
            try
            {
                // Validar que el archivo no esté vacío
                if (request.Archivo == null || request.Archivo.Length == 0)
                    return BadRequest(new { mensaje = "El archivo es requerido", exitoso = false });

                // Validar tamaño de archivo
                var (tamañoValido, mensajeTamaño) = ValidadorDelSistema.ValidarTamañoArchivo(request.Archivo.Length);
                if (!tamañoValido)
                    return BadRequest(new { mensaje = mensajeTamaño, exitoso = false });

                // Validar extensión de archivo
                var (extensionValida, mensajeExtension) = ValidadorDelSistema.ValidarExtensionArchivo(
                    request.Archivo.FileName);
                if (!extensionValida)
                    return BadRequest(new { mensaje = mensajeExtension, exitoso = false });

                // Validar nombre descriptivo
                var (nombreValido, mensajeName) = ValidadorDelSistema.ValidarTituloDocumento(
                    request.NombreDescriptivo);
                if (!nombreValido)
                    return BadRequest(new { mensaje = mensajeName, exitoso = false });

                // Validar descripción si se proporciona
                if (!string.IsNullOrWhiteSpace(request.Descripcion))
                {
                    var (descripcionValida, mensajeDesc) = ValidadorDelSistema.ValidarDescripcion(
                        request.Descripcion);
                    if (!descripcionValida)
                        return BadRequest(new { mensaje = mensajeDesc, exitoso = false });
                }

                // Verificar que el usuario existe y está activo
                var usuarioExiste = await _contextoBD.UsuariosDelSistema
                    .FirstOrDefaultAsync(u => u.IdentificadorUsuario == request.UsuarioId);

                if (usuarioExiste == null)
                    return NotFound(new { mensaje = "Usuario no encontrado", exitoso = false });

                if (!usuarioExiste.IndicadorUsuarioActivo)
                    return BadRequest(new { mensaje = "El usuario está desactivado", exitoso = false });

                // Sanitizar datos
                string nombreLimpio = ValidadorDelSistema.SanitizarTexto(request.NombreDescriptivo);
                string descripcionLimpia = ValidadorDelSistema.SanitizarTexto(request.Descripcion ?? "");

                // Crear carpeta de documentos si no existe
                string carpetaDocumentos = Path.Combine(Directory.GetCurrentDirectory(), "DocumentosSubidos");
                if (!Directory.Exists(carpetaDocumentos))
                    Directory.CreateDirectory(carpetaDocumentos);

                // Generar nombre único para el archivo
                string nombreArchivoUnico = $"{DateTime.UtcNow:yyyyMMddHHmmss}_{Path.GetRandomFileName()}_{Path.GetFileName(request.Archivo.FileName)}";
                string rutaCompleta = Path.Combine(carpetaDocumentos, nombreArchivoUnico);

                // Guardar archivo en el servidor
                using (var streamArchivo = new FileStream(rutaCompleta, FileMode.Create))
                {
                    await request.Archivo.CopyToAsync(streamArchivo);
                }

                // Crear documento en la base de datos
                var nuevoDocumento = new Documento
                {
                    NombreDescriptivoDelDocumento = nombreLimpio,
                    RutaFisicaDelArchivoDocumento = rutaCompleta,
                    DescripcionAdicionalesDelDocumento = descripcionLimpia,
                    IdentificadorUsuarioPropietarioDelDocumento = request.UsuarioId,
                    NumeroVersionActualDelDocumento = 1,
                    FechaSubidaDelDocumento = DateTime.UtcNow,
                    EstadoActualDelDocumento = "Pendiente",
                    FechaUltimaModificacionDelDocumento = DateTime.UtcNow
                };

                _contextoBD.DocumentosDelSistema.Add(nuevoDocumento);
                await _contextoBD.SaveChangesAsync();

                // Registrar en auditoría
                await _servicioAuditoria.RegistrarAccionAuditoria(
                    request.UsuarioId,
                    "Archivo subido al documento",
                    $"Archivo: {nombreArchivoUnico} | Tamaño: {(request.Archivo.Length / 1024.0):F2} KB",
                    nuevoDocumento.IdentificadorDocumento,
                    HttpContext.Connection.RemoteIpAddress?.ToString() ?? ""
                );

                _registradorDeLog.LogInformation($"Documento subido exitosamente: {nombreLimpio} por usuario {request.UsuarioId}");

                return CreatedAtAction(nameof(ObtenerDocumentoPorId), new { id = nuevoDocumento.IdentificadorDocumento },
                    new
                    {
                        mensaje = "Documento subido exitosamente",
                        documentoId = nuevoDocumento.IdentificadorDocumento,
                        nombreArchivo = nombreArchivoUnico,
                        nombreDescriptivo = nombreLimpio,
                        tamañoKB = (request.Archivo.Length / 1024.0),
                        exitoso = true
                    });
            }
            catch (Exception excepcion)
            {
                _registradorDeLog.LogError($"Error subiendo documento: {excepcion.Message}");
                return StatusCode(500, new { mensaje = "Error interno del servidor", exitoso = false });
            }
        }

        /// <summary>
        /// Descargar documento
        /// GET: api/documentos/descargar/{id}
        /// </summary>
        [HttpGet("descargar/{id}")]
        public async Task<ActionResult> DescargarDocumento(int id)
        {
            try
            {
                var documento = await _contextoBD.DocumentosDelSistema
                    .FirstOrDefaultAsync(d => d.IdentificadorDocumento == id);

                if (documento == null)
                    return NotFound(new { mensaje = "Documento no encontrado", exitoso = false });

                if (!System.IO.File.Exists(documento.RutaFisicaDelArchivoDocumento))
                    return NotFound(new { mensaje = "Archivo no encontrado en el servidor", exitoso = false });

                var bytes = await System.IO.File.ReadAllBytesAsync(documento.RutaFisicaDelArchivoDocumento);
                var nombreArchivo = Path.GetFileName(documento.RutaFisicaDelArchivoDocumento);

                _registradorDeLog.LogInformation($"Documento {id} descargado");

                return File(bytes, "application/octet-stream", nombreArchivo);
            }
            catch (Exception excepcion)
            {
                _registradorDeLog.LogError($"Error descargando documento: {excepcion.Message}");
                return StatusCode(500, new { mensaje = "Error interno del servidor", exitoso = false });
            }
        }

        /// <summary>
        /// Eliminar documento
        /// DELETE: api/documentos/{id}
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<object>> EliminarDocumento(int id)
        {
            try
            {
                var documentoAEliminar = await _contextoBD.DocumentosDelSistema
                    .Include(d => d.VersionesDelDocumento)
                    .FirstOrDefaultAsync(d => d.IdentificadorDocumento == id);

                if (documentoAEliminar == null)
                    return NotFound(new { mensaje = "Documento no encontrado", exitoso = false });

                // Eliminar archivo físico si existe
                if (!string.IsNullOrEmpty(documentoAEliminar.RutaFisicaDelArchivoDocumento) && 
                    System.IO.File.Exists(documentoAEliminar.RutaFisicaDelArchivoDocumento))
                {
                    System.IO.File.Delete(documentoAEliminar.RutaFisicaDelArchivoDocumento);
                }

                _contextoBD.DocumentosDelSistema.Remove(documentoAEliminar);
                await _contextoBD.SaveChangesAsync();

                _registradorDeLog.LogInformation($"Documento {id} eliminado");

                return Ok(new { mensaje = "Documento eliminado exitosamente", exitoso = true });
            }
            catch (Exception excepcion)
            {
                _registradorDeLog.LogError($"Error eliminando documento: {excepcion.Message}");
                return StatusCode(500, new { mensaje = "Error interno del servidor", exitoso = false });
            }
        }
    }

    // ==================== MODELOS DTO ====================

    /// <summary>Modelo para crear un nuevo documento</summary>
    public class ModeloCrearDocumento
    {
        public string NombreDescriptivoDelDocumento { get; set; } = "";
        public string RutaFisicaDelArchivoDocumento { get; set; } = "";
        public string DescripcionAdicionalesDelDocumento { get; set; } = "";
        public string TipoDocumento { get; set; } = "";
        public int IdentificadorUsuarioPropietarioDelDocumento { get; set; }
    }

    /// <summary>Modelo para subir un documento con archivo</summary>
    public class SubirDocumentoRequest
    {
        public IFormFile? Archivo { get; set; }
        public string? NombreDescriptivo { get; set; }
        public string? Descripcion { get; set; }
        public int UsuarioId { get; set; }
    }
}