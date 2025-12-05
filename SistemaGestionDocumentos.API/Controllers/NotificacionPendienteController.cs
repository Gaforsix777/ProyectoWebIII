using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaGestionDocumentos.API.Data;
using SistemaGestionDocumentos.API.Models;

namespace SistemaGestionDocumentos.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificacionPendienteController : ControllerBase
    {
        private readonly SistemaGestionDocumentosDbContext _contextoBD;
        private readonly ILogger<NotificacionPendienteController> _registradorDeLog;

        public NotificacionPendienteController(SistemaGestionDocumentosDbContext contextoBD, ILogger<NotificacionPendienteController> registrador)
        {
            _contextoBD = contextoBD;
            _registradorDeLog = registrador;
        }

        /// <summary>
        /// Crear una nueva notificación
        /// POST: api/notificacionpendiente/crear
        /// </summary>
        [HttpPost("crear")]
        public async Task<ActionResult<object>> CrearNotificacion(
            [FromBody] CrearNotificacionDTO datosNotificacion)
        {
            try
            {
                var usuarioReceptor = await _contextoBD.UsuariosDelSistema
                    .FirstOrDefaultAsync(u => u.IdentificadorUsuario == datosNotificacion.IdentificadorUsuarioReceptor);

                if (usuarioReceptor == null)
                    return NotFound(new { mensaje = "Usuario receptor no encontrado", exitoso = false });

                var nuevaNotificacion = new NotificacionPendiente
                {
                    IdentificadorUsuarioReceptorNotificacion = datosNotificacion.IdentificadorUsuarioReceptor,
                    IdentificadorDocumentoRelacionadoNotificacion = datosNotificacion.IdentificadorDocumento,
                    TipoNotificacionEnviar = datosNotificacion.TipoNotificacion,
                    AsuntoNotificacionCorreo = datosNotificacion.Asunto,
                    ContenidoMensajeNotificacion = datosNotificacion.Contenido ?? "",
                    IndicadorNotificacionLeidaONo = false,
                    FechaCreacionNotificacion = DateTime.UtcNow,
                    FechaEnvioNotificacion = null
                };

                _contextoBD.NotificacionesPendientesDelSistema.Add(nuevaNotificacion);
                await _contextoBD.SaveChangesAsync();

                _registradorDeLog.LogInformation($"Notificación creada para usuario {datosNotificacion.IdentificadorUsuarioReceptor}");

                return CreatedAtAction(nameof(ObtenerNotificacion),
                    new { id = nuevaNotificacion.IdentificadorNotificacionPendiente },
                    new { mensaje = "Notificación creada", exitoso = true });
            }
            catch (Exception excepcion)
            {
                _registradorDeLog.LogError($"Error creando notificación: {excepcion.Message}");
                return StatusCode(500, new { mensaje = "Error interno del servidor", exitoso = false });
            }
        }

        /// <summary>
        /// Obtener todas las notificaciones de un usuario
        /// GET: api/notificacionpendiente/usuario/{usuarioId}
        /// </summary>
        [HttpGet("usuario/{usuarioId}")]
        public async Task<ActionResult<IEnumerable<object>>> ObtenerNotificacionesDelUsuario(int usuarioId)
        {
            try
            {
                var notificaciones = await _contextoBD.NotificacionesPendientesDelSistema
                    .Where(n => n.IdentificadorUsuarioReceptorNotificacion == usuarioId)
                    .OrderByDescending(n => n.FechaCreacionNotificacion)
                    .Select(n => new
                    {
                        n.IdentificadorNotificacionPendiente,
                        n.TipoNotificacionEnviar,
                        n.AsuntoNotificacionCorreo,
                        n.ContenidoMensajeNotificacion,
                        n.IndicadorNotificacionLeidaONo,
                        n.FechaCreacionNotificacion,
                        n.IdentificadorDocumentoRelacionadoNotificacion
                    })
                    .ToListAsync();

                return Ok(notificaciones);
            }
            catch (Exception excepcion)
            {
                _registradorDeLog.LogError($"Error obteniendo notificaciones: {excepcion.Message}");
                return StatusCode(500, new { mensaje = "Error interno del servidor", exitoso = false });
            }
        }

        /// <summary>
        /// Obtener una notificación específica
        /// GET: api/notificacionpendiente/{id}
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> ObtenerNotificacion(int id)
        {
            try
            {
                var notificacion = await _contextoBD.NotificacionesPendientesDelSistema
                    .FirstOrDefaultAsync(n => n.IdentificadorNotificacionPendiente == id);

                if (notificacion == null)
                    return NotFound(new { mensaje = "Notificación no encontrada", exitoso = false });

                return Ok(new
                {
                    notificacion.IdentificadorNotificacionPendiente,
                    notificacion.TipoNotificacionEnviar,
                    notificacion.AsuntoNotificacionCorreo,
                    notificacion.ContenidoMensajeNotificacion,
                    notificacion.IndicadorNotificacionLeidaONo,
                    notificacion.FechaCreacionNotificacion,
                    notificacion.FechaEnvioNotificacion
                });
            }
            catch (Exception excepcion)
            {
                _registradorDeLog.LogError($"Error obteniendo notificación: {excepcion.Message}");
                return StatusCode(500, new { mensaje = "Error interno del servidor", exitoso = false });
            }
        }

        /// <summary>
        /// Marcar notificación como leída
        /// PUT: api/notificacionpendiente/{id}/marcar-leida
        /// </summary>
        [HttpPut("{id}/marcar-leida")]
        public async Task<ActionResult<object>> MarcarComoLeida(int id)
        {
            try
            {
                var notificacion = await _contextoBD.NotificacionesPendientesDelSistema
                    .FirstOrDefaultAsync(n => n.IdentificadorNotificacionPendiente == id);

                if (notificacion == null)
                    return NotFound(new { mensaje = "Notificación no encontrada", exitoso = false });

                notificacion.IndicadorNotificacionLeidaONo = true;
                notificacion.FechaEnvioNotificacion = DateTime.UtcNow;

                _contextoBD.NotificacionesPendientesDelSistema.Update(notificacion);
                await _contextoBD.SaveChangesAsync();

                return Ok(new { mensaje = "Notificación marcada como leída", exitoso = true });
            }
            catch (Exception excepcion)
            {
                _registradorDeLog.LogError($"Error marcando notificación: {excepcion.Message}");
                return StatusCode(500, new { mensaje = "Error interno del servidor", exitoso = false });
            }
        }

        /// <summary>
        /// Obtener notificaciones no leídas de un usuario
        /// GET: api/notificacionpendiente/usuario/{usuarioId}/no-leidas
        /// </summary>
        [HttpGet("usuario/{usuarioId}/no-leidas")]
        public async Task<ActionResult<IEnumerable<object>>> ObtenerNotificacionesNoLeidas(int usuarioId)
        {
            try
            {
                var notificacionesNoLeidas = await _contextoBD.NotificacionesPendientesDelSistema
                    .Where(n => n.IdentificadorUsuarioReceptorNotificacion == usuarioId && !n.IndicadorNotificacionLeidaONo)
                    .OrderByDescending(n => n.FechaCreacionNotificacion)
                    .Select(n => new
                    {
                        n.IdentificadorNotificacionPendiente,
                        n.TipoNotificacionEnviar,
                        n.AsuntoNotificacionCorreo,
                        n.ContenidoMensajeNotificacion,
                        n.FechaCreacionNotificacion
                    })
                    .ToListAsync();

                return Ok(notificacionesNoLeidas);
            }
            catch (Exception excepcion)
            {
                _registradorDeLog.LogError($"Error obteniendo notificaciones no leídas: {excepcion.Message}");
                return StatusCode(500, new { mensaje = "Error interno del servidor", exitoso = false });
            }
        }

        /// <summary>
        /// Eliminar una notificación
        /// DELETE: api/notificacionpendiente/{id}
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<object>> EliminarNotificacion(int id)
        {
            try
            {
                var notificacion = await _contextoBD.NotificacionesPendientesDelSistema
                    .FirstOrDefaultAsync(n => n.IdentificadorNotificacionPendiente == id);

                if (notificacion == null)
                    return NotFound(new { mensaje = "Notificación no encontrada", exitoso = false });

                _contextoBD.NotificacionesPendientesDelSistema.Remove(notificacion);
                await _contextoBD.SaveChangesAsync();

                return Ok(new { mensaje = "Notificación eliminada", exitoso = true });
            }
            catch (Exception excepcion)
            {
                _registradorDeLog.LogError($"Error eliminando notificación: {excepcion.Message}");
                return StatusCode(500, new { mensaje = "Error interno del servidor", exitoso = false });
            }
        }
    }

   public class CrearNotificacionDTO

{

    public int IdentificadorUsuarioReceptor { get; set; }

    public int? IdentificadorDocumento { get; set; }

    public string TipoNotificacion { get; set; } = "";

    public string Asunto { get; set; } = "";

    public string? Contenido { get; set; }

}
}
