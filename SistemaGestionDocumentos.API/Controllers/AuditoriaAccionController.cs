using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaGestionDocumentos.API.Data;
using SistemaGestionDocumentos.API.Models;

namespace SistemaGestionDocumentos.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuditoriaAccionController : ControllerBase
    {
        private readonly SistemaGestionDocumentosDbContext _contextoBD;
        private readonly ILogger<AuditoriaAccionController> _registradorDeLog;

        public AuditoriaAccionController(SistemaGestionDocumentosDbContext contextoBD, ILogger<AuditoriaAccionController> registrador)
        {
            _contextoBD = contextoBD;
            _registradorDeLog = registrador;
        }

        /// <summary>
        /// Registrar una acción en auditoría
        /// POST: api/auditoriaaccion/registrar
        /// </summary>
        [HttpPost("registrar")]
        public async Task<ActionResult<object>> RegistrarAccionAuditoria(
            [FromBody] RegistroAuditoriaDTO datosAuditoria)
        {
            try
            {
                var usuarioExistente = await _contextoBD.UsuariosDelSistema
                    .FirstOrDefaultAsync(u => u.IdentificadorUsuario == datosAuditoria.IdentificadorUsuario);

                if (usuarioExistente == null)
                    return NotFound(new { mensaje = "Usuario no encontrado", exitoso = false });

                var nuevoRegistro = new AuditoriaAccion
                {
                    IdentificadorUsuarioQueRealizaAccion = datosAuditoria.IdentificadorUsuario,
                    DescripcionAccionRealizada = datosAuditoria.DescripcionAccion,
                    FechaHoraExactaAccion = DateTime.UtcNow,
                    DetallesAdicionalesAccion = datosAuditoria.Detalles ?? "",
                    IdentificadorDocumentoRelacionadoAccion = datosAuditoria.IdentificadorDocumento,
                    DireccionIPDelDispositivoAccion = datosAuditoria.DireccionIP ?? "No especificada"
                };

                _contextoBD.AuditoriaAccionDelSistema.Add(nuevoRegistro);
                await _contextoBD.SaveChangesAsync();

                _registradorDeLog.LogInformation($"Acción auditada: {datosAuditoria.DescripcionAccion} por usuario {datosAuditoria.IdentificadorUsuario}");

                return CreatedAtAction(nameof(ObtenerRegistroAuditoria),
                    new { id = nuevoRegistro.IdentificadorRegistroAuditoria },
                    new { mensaje = "Acción registrada en auditoría", exitoso = true });
            }
            catch (Exception excepcion)
            {
                _registradorDeLog.LogError($"Error registrando auditoría: {excepcion.Message}");
                return StatusCode(500, new { mensaje = "Error interno del servidor", exitoso = false });
            }
        }

        /// <summary>
        /// Obtener todos los registros de auditoría
        /// GET: api/auditoriaaccion
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> ObtenerTodosLosRegistros()
        {
            try
            {
                var registros = await _contextoBD.AuditoriaAccionDelSistema
                    .Include(a => a.UsuarioQueRealiza)
                    .OrderByDescending(a => a.FechaHoraExactaAccion)
                    .Select(a => new
                    {
                        a.IdentificadorRegistroAuditoria,
                        a.DescripcionAccionRealizada,
                        a.FechaHoraExactaAccion,
                        a.DetallesAdicionalesAccion,
                        a.DireccionIPDelDispositivoAccion,
                        Usuario = a.UsuarioQueRealiza.NombreCompletoDelUsuario,
                        a.IdentificadorDocumentoRelacionadoAccion
                    })
                    .ToListAsync();

                return Ok(registros);
            }
            catch (Exception excepcion)
            {
                _registradorDeLog.LogError($"Error obteniendo auditoría: {excepcion.Message}");
                return StatusCode(500, new { mensaje = "Error interno del servidor", exitoso = false });
            }
        }

        /// <summary>
        /// Obtener un registro de auditoría específico
        /// GET: api/auditoriaaccion/{id}
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> ObtenerRegistroAuditoria(int id)
        {
            try
            {
                var registro = await _contextoBD.AuditoriaAccionDelSistema
                    .Include(a => a.UsuarioQueRealiza)
                    .FirstOrDefaultAsync(a => a.IdentificadorRegistroAuditoria == id);

                if (registro == null)
                    return NotFound(new { mensaje = "Registro de auditoría no encontrado", exitoso = false });

                return Ok(new
                {
                    registro.IdentificadorRegistroAuditoria,
                    registro.DescripcionAccionRealizada,
                    registro.FechaHoraExactaAccion,
                    registro.DetallesAdicionalesAccion,
                    registro.DireccionIPDelDispositivoAccion,
                    Usuario = registro.UsuarioQueRealiza.NombreCompletoDelUsuario,
                    registro.IdentificadorDocumentoRelacionadoAccion
                });
            }
            catch (Exception excepcion)
            {
                _registradorDeLog.LogError($"Error obteniendo registro: {excepcion.Message}");
                return StatusCode(500, new { mensaje = "Error interno del servidor", exitoso = false });
            }
        }

        /// <summary>
        /// Obtener auditoría de un usuario específico
        /// GET: api/auditoriaaccion/por-usuario/{usuarioId}
        /// </summary>
        [HttpGet("por-usuario/{usuarioId}")]
        public async Task<ActionResult<IEnumerable<object>>> ObtenerAuditoriaDelUsuario(int usuarioId)
        {
            try
            {
                var registros = await _contextoBD.AuditoriaAccionDelSistema
                    .Where(a => a.IdentificadorUsuarioQueRealizaAccion == usuarioId)
                    .OrderByDescending(a => a.FechaHoraExactaAccion)
                    .Select(a => new
                    {
                        a.IdentificadorRegistroAuditoria,
                        a.DescripcionAccionRealizada,
                        a.FechaHoraExactaAccion,
                        a.DetallesAdicionalesAccion
                    })
                    .ToListAsync();

                return Ok(registros);
            }
            catch (Exception excepcion)
            {
                _registradorDeLog.LogError($"Error obteniendo auditoría del usuario: {excepcion.Message}");
                return StatusCode(500, new { mensaje = "Error interno del servidor", exitoso = false });
            }
        }
    }

    public class RegistroAuditoriaDTO
    {
        public int IdentificadorUsuario { get; set; }
        public string DescripcionAccion { get; set; }
        public string? Detalles { get; set; }
        public int? IdentificadorDocumento { get; set; }
        public string? DireccionIP { get; set; }
    }
}
