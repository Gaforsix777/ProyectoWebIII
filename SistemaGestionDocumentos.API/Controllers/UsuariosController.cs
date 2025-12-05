using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaGestionDocumentos.API.Data;
using SistemaGestionDocumentos.API.Models;
using System.Security.Cryptography;
using System.Text;

namespace SistemaGestionDocumentos.API.Controllers
{
    /// <summary>
    /// Controlador para gestionar usuarios del sistema
    /// Incluye operaciones CRUD y autenticación
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly SistemaGestionDocumentosDbContext _contextoBD;
        private readonly ILogger<UsuariosController> _registradorDeLog;

        /// <summary>Constructor con inyección de dependencias</summary>
        public UsuariosController(SistemaGestionDocumentosDbContext contextoBD, ILogger<UsuariosController> registrador)
        {
            _contextoBD = contextoBD;
            _registradorDeLog = registrador;
        }

        /// <summary>
        /// Endpoint para registrar un nuevo usuario
        /// POST: api/usuarios/registrar
        /// </summary>
        [HttpPost("registrar")]
        public async Task<ActionResult<object>> RegistrarNuevoUsuario(
            [FromBody] ModeloRegistroUsuario datosPeticionRegistro)
        {
            try
            {
                var usuarioYaExiste = await _contextoBD.UsuariosDelSistema
                    .FirstOrDefaultAsync(u => u.CorreoElectronicoDelUsuario == datosPeticionRegistro.CorreoElectronico);

                if (usuarioYaExiste != null)
                    return BadRequest(new { mensaje = "El correo ya está registrado en el sistema", exitoso = false });

                var contraseñaHasheada = ObtenerHashDelaSHA256(datosPeticionRegistro.Contraseña);

                var nuevoUsuario = new Usuario
                {
                    CorreoElectronicoDelUsuario = datosPeticionRegistro.CorreoElectronico,
                    ContraseñaHashDelUsuario = contraseñaHasheada,
                    NombreCompletoDelUsuario = datosPeticionRegistro.NombreCompleto,
                    RolDelUsuario = datosPeticionRegistro.Rol ?? "Solicitante",
                    FechaCreacionDelUsuario = DateTime.UtcNow,
                    IndicadorUsuarioActivo = true
                };

                _contextoBD.UsuariosDelSistema.Add(nuevoUsuario);
                await _contextoBD.SaveChangesAsync();

                _registradorDeLog.LogInformation($"Nuevo usuario registrado: {datosPeticionRegistro.CorreoElectronico}");

                return CreatedAtAction(nameof(ObtenerUsuarioPorId), new { id = nuevoUsuario.IdentificadorUsuario },
                    new { mensaje = "Usuario registrado exitosamente", usuarioId = nuevoUsuario.IdentificadorUsuario, exitoso = true });
            }
            catch (Exception excepcion)
            {
                _registradorDeLog.LogError($"Error registrando usuario: {excepcion.Message}");
                return StatusCode(500, new { mensaje = "Error interno del servidor", exitoso = false });
            }
        }

        /// <summary>
        /// Endpoint para login de usuario
        /// POST: api/usuarios/login
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<object>> LoginDeUsuario(
            [FromBody] ModeloLoginUsuario datosLogin)
        {
            try
            {
                var usuarioEncontrado = await _contextoBD.UsuariosDelSistema
                    .FirstOrDefaultAsync(u => u.CorreoElectronicoDelUsuario == datosLogin.CorreoElectronico);

                if (usuarioEncontrado == null)
                    return Unauthorized(new { mensaje = "Correo o contraseña inválidos", exitoso = false });

                var contraseñaDelSistemaHasheada = ObtenerHashDelaSHA256(datosLogin.Contraseña);
                if (usuarioEncontrado.ContraseñaHashDelUsuario != contraseñaDelSistemaHasheada)
                    return Unauthorized(new { mensaje = "Correo o contraseña inválidos", exitoso = false });

                if (!usuarioEncontrado.IndicadorUsuarioActivo)
                    return Unauthorized(new { mensaje = "El usuario ha sido desactivado", exitoso = false });

                // Generar token simple
                string token = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(
                    $"{usuarioEncontrado.IdentificadorUsuario}:{usuarioEncontrado.CorreoElectronicoDelUsuario}"
                ));

                _registradorDeLog.LogInformation($"Usuario login exitoso: {datosLogin.CorreoElectronico}");

                return Ok(new
                {
                    mensaje = "Login exitoso",
                    exitoso = true,
                    token = token,
                    usuario = new
                    {
                        identificadorUsuario = usuarioEncontrado.IdentificadorUsuario,
                        correoElectronicoDelUsuario = usuarioEncontrado.CorreoElectronicoDelUsuario,
                        nombreCompletoDelUsuario = usuarioEncontrado.NombreCompletoDelUsuario,
                        rolDelUsuario = usuarioEncontrado.RolDelUsuario
                    }
                });
            }
            catch (Exception excepcion)
            {
                _registradorDeLog.LogError($"Error en login: {excepcion.Message}");
                return StatusCode(500, new { mensaje = "Error interno del servidor", exitoso = false });
            }
        }

        /// <summary>
        /// Endpoint para obtener todos los usuarios
        /// GET: api/usuarios
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> ObtenerTodosLosUsuarios()
        {
            try
            {
                var usuariosDelSistema = await _contextoBD.UsuariosDelSistema
                    .Where(u => u.IndicadorUsuarioActivo)
                    .Select(u => new
                    {
                        u.IdentificadorUsuario,
                        u.CorreoElectronicoDelUsuario,
                        u.NombreCompletoDelUsuario,
                        u.RolDelUsuario,
                        u.FechaCreacionDelUsuario
                    })
                    .ToListAsync();

                return Ok(usuariosDelSistema);
            }
            catch (Exception excepcion)
            {
                _registradorDeLog.LogError($"Error obteniendo usuarios: {excepcion.Message}");
                return StatusCode(500, new { mensaje = "Error interno del servidor", exitoso = false });
            }
        }

        /// <summary>
        /// Endpoint para obtener un usuario por ID
        /// GET: api/usuarios/{id}
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> ObtenerUsuarioPorId(int id)
        {
            try
            {
                var usuarioEncontrado = await _contextoBD.UsuariosDelSistema
                    .FirstOrDefaultAsync(u => u.IdentificadorUsuario == id);

                if (usuarioEncontrado == null)
                    return NotFound(new { mensaje = "Usuario no encontrado", exitoso = false });

                return Ok(new
                {
                    usuarioEncontrado.IdentificadorUsuario,
                    usuarioEncontrado.CorreoElectronicoDelUsuario,
                    usuarioEncontrado.NombreCompletoDelUsuario,
                    usuarioEncontrado.RolDelUsuario,
                    usuarioEncontrado.FechaCreacionDelUsuario,
                    usuarioEncontrado.IndicadorUsuarioActivo
                });
            }
            catch (Exception excepcion)
            {
                _registradorDeLog.LogError($"Error obteniendo usuario: {excepcion.Message}");
                return StatusCode(500, new { mensaje = "Error interno del servidor", exitoso = false });
            }
        }

        /// <summary>
        /// Endpoint para actualizar rol de usuario (solo Admin)
        /// PUT: api/usuarios/{id}/actualizar-rol
        /// </summary>
        [HttpPut("{id}/actualizar-rol")]
        public async Task<ActionResult<object>> ActualizarRolDelUsuario(
            int id,
            [FromBody] ModeloActualizacionRolUsuario datosActualizacion)
        {
            try
            {
                var usuarioAActualizar = await _contextoBD.UsuariosDelSistema
                    .FirstOrDefaultAsync(u => u.IdentificadorUsuario == id);

                if (usuarioAActualizar == null)
                    return NotFound(new { mensaje = "Usuario no encontrado", exitoso = false });

                var rolAnterior = usuarioAActualizar.RolDelUsuario;
                usuarioAActualizar.RolDelUsuario = datosActualizacion.NuevoRol;

                _contextoBD.UsuariosDelSistema.Update(usuarioAActualizar);
                await _contextoBD.SaveChangesAsync();

                _registradorDeLog.LogInformation($"Rol del usuario {id} actualizado de {rolAnterior} a {datosActualizacion.NuevoRol}");

                return Ok(new { mensaje = "Rol actualizado exitosamente", exitoso = true });
            }
            catch (Exception excepcion)
            {
                _registradorDeLog.LogError($"Error actualizando rol: {excepcion.Message}");
                return StatusCode(500, new { mensaje = "Error interno del servidor", exitoso = false });
            }
        }

        /// <summary>
        /// Endpoint para desactivar un usuario
        /// DELETE: api/usuarios/{id}/desactivar
        /// </summary>
        [HttpDelete("{id}/desactivar")]
        public async Task<ActionResult<object>> DesactivarUsuario(int id)
        {
            try
            {
                var usuarioADesactivar = await _contextoBD.UsuariosDelSistema
                    .FirstOrDefaultAsync(u => u.IdentificadorUsuario == id);

                if (usuarioADesactivar == null)
                    return NotFound(new { mensaje = "Usuario no encontrado", exitoso = false });

                usuarioADesactivar.IndicadorUsuarioActivo = false;
                _contextoBD.UsuariosDelSistema.Update(usuarioADesactivar);
                await _contextoBD.SaveChangesAsync();

                _registradorDeLog.LogInformation($"Usuario {id} desactivado");

                return Ok(new { mensaje = "Usuario desactivado exitosamente", exitoso = true });
            }
            catch (Exception excepcion)
            {
                _registradorDeLog.LogError($"Error desactivando usuario: {excepcion.Message}");
                return StatusCode(500, new { mensaje = "Error interno del servidor", exitoso = false });
            }
        }

        /// <summary>
        /// Método privado para crear hash SHA256 de una contraseña
        /// </summary>
        private string ObtenerHashDelaSHA256(string textoAConvertir)
        {
            using (var algoritmoHash = SHA256.Create())
            {
                var bytesDeTExto = Encoding.UTF8.GetBytes(textoAConvertir);
                var bytesDelHash = algoritmoHash.ComputeHash(bytesDeTExto);
                return Convert.ToBase64String(bytesDelHash);
            }
        }
    }

    // ==================== MODELOS DTO ====================

    /// <summary>Modelo para el registro de un nuevo usuario</summary>
    public class ModeloRegistroUsuario
    {
        public string CorreoElectronico { get; set; }
        public string Contraseña { get; set; }
        public string NombreCompleto { get; set; }
        public string Rol { get; set; }
    }

    /// <summary>Modelo para el login de usuario</summary>
    public class ModeloLoginUsuario
    {
        public string CorreoElectronico { get; set; }
        public string Contraseña { get; set; }
    }

    /// <summary>Modelo para actualizar rol de usuario</summary>
    public class ModeloActualizacionRolUsuario
    {
        public string NuevoRol { get; set; }
    }
}