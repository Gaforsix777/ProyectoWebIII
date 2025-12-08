using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaGestionDocumentos.API.Data;
using SistemaGestionDocumentos.API.Models;
using SistemaGestionDocumentos.API.Services;

namespace SistemaGestionDocumentos.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkflowAprobacionController : ControllerBase
    {
        private readonly SistemaGestionDocumentosDbContext _contextoBD;
        private readonly ILogger<WorkflowAprobacionController> _registradorDeLog;
        private readonly AuditoriaService _servicioAuditoria;

        public WorkflowAprobacionController(SistemaGestionDocumentosDbContext contextoBD, ILogger<WorkflowAprobacionController> registrador, AuditoriaService servicioAuditoria)
        {
            _contextoBD = contextoBD;
            _registradorDeLog = registrador;
            _servicioAuditoria = servicioAuditoria;
        }

        /// <summary>
        /// Solicitar aprobación de un documento
        /// POST: api/workflowaprobacion/solicitar-aprobacion
        /// </summary>
        [HttpPost("solicitar-aprobacion")]
        public async Task<ActionResult<object>> SolicitarAprobacionDocumento(
            [FromBody] SolicitudAprobacionDTO datosAprobacion)
        {
            try
            {
                var documentoExistente = await _contextoBD.DocumentosDelSistema
                    .FirstOrDefaultAsync(d => d.IdentificadorDocumento == datosAprobacion.IdentificadorDocumento);

                if (documentoExistente == null)
                    return NotFound(new { mensaje = "Documento no encontrado", exitoso = false });

                var usuarioAprobador = await _contextoBD.UsuariosDelSistema
                    .FirstOrDefaultAsync(u => u.IdentificadorUsuario == datosAprobacion.IdentificadorUsuarioAprobador);

                if (usuarioAprobador == null)
                    return NotFound(new { mensaje = "Usuario aprobador no encontrado", exitoso = false });

                var registroAprobacion = new WorkflowAprobacion
                {
                    IdentificadorDocumentoAprobar = datosAprobacion.IdentificadorDocumento,
                    EstadoActualDelFlujoAprobacion = "Solicitado",
                    IdentificadorUsuarioAprobadorDelDocumento = datosAprobacion.IdentificadorUsuarioAprobador,
                    FechaAccionAprobacionDelDocumento = DateTime.UtcNow,
                    ComentariosDelAprobadorDelDocumento = datosAprobacion.Comentarios ?? "",
                    NivelPrioridadDelDocumento = datosAprobacion.Prioridad ?? "Media"
                };

                _contextoBD.WorkflowAprobacionDelSistema.Add(registroAprobacion);

                // Actualizar estado del documento
                documentoExistente.EstadoActualDelDocumento = "En Revisión";
                _contextoBD.DocumentosDelSistema.Update(documentoExistente);

                await _contextoBD.SaveChangesAsync();

                // Registrar en auditoría - usuario que propietario del documento
                await _servicioAuditoria.RegistrarAccionAuditoria(
                    documentoExistente.IdentificadorUsuarioPropietarioDelDocumento,
                    $"Solicitud de aprobación creada para documento {datosAprobacion.IdentificadorDocumento}",
                    $"Aprobador: {usuarioAprobador.NombreCompletoDelUsuario}, Prioridad: {datosAprobacion.Prioridad ?? "Media"}",
                    datosAprobacion.IdentificadorDocumento,
                    HttpContext.Connection.RemoteIpAddress?.ToString() ?? ""
                );

                _registradorDeLog.LogInformation($"Documento {datosAprobacion.IdentificadorDocumento} solicitado para aprobación");

                return CreatedAtAction(nameof(ObtenerRegistroAprobacion), 
                    new { id = registroAprobacion.IdentificadorRegistroAprobacion },
                    new { mensaje = "Solicitud de aprobación creada", exitoso = true });
            }
            catch (Exception excepcion)
            {
                _registradorDeLog.LogError($"Error solicitando aprobación: {excepcion.Message}");
                return StatusCode(500, new { mensaje = "Error interno del servidor", exitoso = false });
            }
        }

        /// <summary>
        /// Aprobar un documento
        /// PUT: api/workflowaprobacion/{id}/aprobar
        /// </summary>
        [HttpPut("{id}/aprobar")]
        public async Task<ActionResult<object>> AprobarDocumento(
            int id,
            [FromBody] RespuestaAprobacionDTO datosRespuesta)
        {
            try
            {
                var registroAprobacion = await _contextoBD.WorkflowAprobacionDelSistema
                    .FirstOrDefaultAsync(w => w.IdentificadorRegistroAprobacion == id);

                if (registroAprobacion == null)
                    return NotFound(new { mensaje = "Registro de aprobación no encontrado", exitoso = false });

                registroAprobacion.EstadoActualDelFlujoAprobacion = "Aprobado";
                registroAprobacion.ComentariosDelAprobadorDelDocumento = datosRespuesta.Comentarios ?? "";
                registroAprobacion.FechaAccionAprobacionDelDocumento = DateTime.UtcNow;

                var documento = await _contextoBD.DocumentosDelSistema
                    .FirstOrDefaultAsync(d => d.IdentificadorDocumento == registroAprobacion.IdentificadorDocumentoAprobar);

                if (documento != null)
                {
                    documento.EstadoActualDelDocumento = "Aprobado";
                    _contextoBD.DocumentosDelSistema.Update(documento);
                }

                _contextoBD.WorkflowAprobacionDelSistema.Update(registroAprobacion);
                await _contextoBD.SaveChangesAsync();

                // Registrar en auditoría - el aprobador registra su aprobación
                await _servicioAuditoria.RegistrarAccionAuditoria(
                    registroAprobacion.IdentificadorUsuarioAprobadorDelDocumento,
                    $"Documento {registroAprobacion.IdentificadorDocumentoAprobar} aprobado",
                    $"Comentarios: {datosRespuesta.Comentarios ?? "Sin comentarios"}",
                    registroAprobacion.IdentificadorDocumentoAprobar,
                    HttpContext.Connection.RemoteIpAddress?.ToString() ?? ""
                );

                _registradorDeLog.LogInformation($"Documento aprobado por usuario {registroAprobacion.IdentificadorUsuarioAprobadorDelDocumento}");

                return Ok(new { mensaje = "Documento aprobado exitosamente", exitoso = true });
            }
            catch (Exception excepcion)
            {
                _registradorDeLog.LogError($"Error aprobando documento: {excepcion.Message}");
                return StatusCode(500, new { mensaje = "Error interno del servidor", exitoso = false });
            }
        }

        /// <summary>
        /// Rechazar un documento
        /// PUT: api/workflowaprobacion/{id}/rechazar
        /// </summary>
        [HttpPut("{id}/rechazar")]
        public async Task<ActionResult<object>> RechazarDocumento(
            int id,
            [FromBody] RespuestaAprobacionDTO datosRespuesta)
        {
            try
            {
                var registroAprobacion = await _contextoBD.WorkflowAprobacionDelSistema
                    .FirstOrDefaultAsync(w => w.IdentificadorRegistroAprobacion == id);

                if (registroAprobacion == null)
                    return NotFound(new { mensaje = "Registro de aprobación no encontrado", exitoso = false });

                registroAprobacion.EstadoActualDelFlujoAprobacion = "Rechazado";
                registroAprobacion.ComentariosDelAprobadorDelDocumento = datosRespuesta.Comentarios ?? "";
                registroAprobacion.FechaAccionAprobacionDelDocumento = DateTime.UtcNow;

                var documento = await _contextoBD.DocumentosDelSistema
                    .FirstOrDefaultAsync(d => d.IdentificadorDocumento == registroAprobacion.IdentificadorDocumentoAprobar);

                if (documento != null)
                {
                    documento.EstadoActualDelDocumento = "Rechazado";
                    _contextoBD.DocumentosDelSistema.Update(documento);
                }

                _contextoBD.WorkflowAprobacionDelSistema.Update(registroAprobacion);
                await _contextoBD.SaveChangesAsync();

                _registradorDeLog.LogInformation($"Documento rechazado por usuario {registroAprobacion.IdentificadorUsuarioAprobadorDelDocumento}");

                return Ok(new { mensaje = "Documento rechazado", exitoso = true });
            }
            catch (Exception excepcion)
            {
                _registradorDeLog.LogError($"Error rechazando documento: {excepcion.Message}");
                return StatusCode(500, new { mensaje = "Error interno del servidor", exitoso = false });
            }
        }

        /// <summary>
        /// Obtener un registro de aprobación
        /// GET: api/workflowaprobacion/{id}
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> ObtenerRegistroAprobacion(int id)
        {
            try
            {
                var registroAprobacion = await _contextoBD.WorkflowAprobacionDelSistema
                    .Include(w => w.DocumentoEnAprobacion)
                    .Include(w => w.UsuarioQueAprueba)
                    .FirstOrDefaultAsync(w => w.IdentificadorRegistroAprobacion == id);

                if (registroAprobacion == null)
                    return NotFound(new { mensaje = "Registro de aprobación no encontrado", exitoso = false });

                return Ok(new
                {
                    registroAprobacion.IdentificadorRegistroAprobacion,
                    registroAprobacion.EstadoActualDelFlujoAprobacion,
                    registroAprobacion.NivelPrioridadDelDocumento,
                    registroAprobacion.ComentariosDelAprobadorDelDocumento,
                    registroAprobacion.FechaAccionAprobacionDelDocumento,
                    Documento = registroAprobacion.DocumentoEnAprobacion.NombreDescriptivoDelDocumento,
                    Aprobador = registroAprobacion.UsuarioQueAprueba.NombreCompletoDelUsuario
                });
            }
            catch (Exception excepcion)
            {
                _registradorDeLog.LogError($"Error obteniendo registro: {excepcion.Message}");
                return StatusCode(500, new { mensaje = "Error interno del servidor", exitoso = false });
            }
        }

        /// <summary>
        /// Obtener todas las aprobaciones pendientes
        /// GET: api/workflowaprobacion/pendientes/todos
        /// </summary>
        [HttpGet("pendientes/todos")]
        public async Task<ActionResult<IEnumerable<object>>> ObtenerAprobacionesPendientes()
        {
            try
            {
                var aprobacionesPendientes = await _contextoBD.WorkflowAprobacionDelSistema
                    .Where(w => w.EstadoActualDelFlujoAprobacion == "Solicitado")
                    .Include(w => w.DocumentoEnAprobacion)
                    .Include(w => w.UsuarioQueAprueba)
                    .Select(w => new
                    {
                        w.IdentificadorRegistroAprobacion,
                        w.EstadoActualDelFlujoAprobacion,
                        w.NivelPrioridadDelDocumento,
                        w.FechaAccionAprobacionDelDocumento,
                        Documento = w.DocumentoEnAprobacion.NombreDescriptivoDelDocumento,
                        Aprobador = w.UsuarioQueAprueba.NombreCompletoDelUsuario
                    })
                    .ToListAsync();

                return Ok(aprobacionesPendientes);
            }
            catch (Exception excepcion)
            {
                _registradorDeLog.LogError($"Error obteniendo aprobaciones pendientes: {excepcion.Message}");
                return StatusCode(500, new { mensaje = "Error interno del servidor", exitoso = false });
            }
        }
    }

    // DTOs
    public class SolicitudAprobacionDTO
    {
        public int IdentificadorDocumento { get; set; }
        public int IdentificadorUsuarioAprobador { get; set; }
        public string? Comentarios { get; set; }
        public string? Prioridad { get; set; }
    }

    public class RespuestaAprobacionDTO
    {
        public string? Comentarios { get; set; }
    }
}
