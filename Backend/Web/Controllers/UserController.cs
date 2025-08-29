using Business;
using Entity.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using Utilities.Exceptions;
using ValidationException = Utilities.Exceptions.ValidationException;

namespace Web.Controllers
{
    /// <summary>
    /// Controlador para la gestión de Users con soporte multi-base de datos
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize] // Todos los endpoints requieren autenticación JWT
    public class UserController : ControllerBase
    {
        private readonly UserBusiness _userBusiness;
        private readonly ILogger<UserController> _logger;

        public UserController(UserBusiness userBusiness, ILogger<UserController> logger)
        {
            _userBusiness = userBusiness;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los usuarios del sistema
        /// </summary>
        /// <returns>Lista de usuarios</returns>
        /// <response code="200">Retorna la lista de usuarios</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
        {
            try
            {
                _logger.LogInformation("Iniciando obtención de todos los usuarios");
                var users = await _userBusiness.GetAllUsersAsync();
                _logger.LogInformation("Se obtuvieron {Count} usuarios", users.Count());
                return Ok(users);
            }
            catch (ExternalServiceException ex)
            {
                _logger.LogError(ex, "Error de servicio externo al obtener usuarios");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "Error al acceder a la base de datos", detail = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al obtener todos los usuarios");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene un usuario específico por su ID
        /// </summary>
        /// <param name="id">ID del usuario</param>
        /// <returns>Usuario encontrado</returns>
        /// <response code="200">Retorna el usuario encontrado</response>
        /// <response code="400">ID inválido</response>
        /// <response code="404">Usuario no encontrado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserDto>> GetUserById([Range(1, int.MaxValue)] int id)
        {
            try
            {
                _logger.LogInformation("Obteniendo usuario con ID: {Id}", id);
                var user = await _userBusiness.GetUserByIdAsync(id);
                return Ok(user);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning("Validación fallida para ID: {Id}. Error: {Error}", id, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogWarning("Usuario no encontrado con ID: {Id}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (ExternalServiceException ex)
            {
                _logger.LogError(ex, "Error de servicio externo al obtener usuario con ID: {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "Error al acceder a la base de datos" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al obtener usuario con ID: {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Crea un nuevo usuario en todas las bases de datos
        /// </summary>
        /// <param name="userDto">Datos del usuario a crear</param>
        /// <returns>Usuario creado</returns>
        /// <response code="201">Usuario creado exitosamente</response>
        /// <response code="400">Datos inválidos</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPost]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserDto>> CreateUser([FromBody] UserDto userDto)
        {
            try
            {
                _logger.LogInformation("Creando nuevo usuario: {UserName}", userDto?.UserName);
                var createdUser = await _userBusiness.CreateUserAsync(userDto);
                
                _logger.LogInformation("Usuario creado exitosamente con ID: {Id}", createdUser.Id);
                return CreatedAtAction(
                    nameof(GetUserById), 
                    new { id = createdUser.Id }, 
                    createdUser);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning("Validación fallida al crear usuario: {Error}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (ExternalServiceException ex)
            {
                _logger.LogError(ex, "Error de servicio externo al crear usuario");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "Error al acceder a la base de datos" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al crear usuario");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "Error interno del servidor al crear usuario" });
            }
        }

        /// <summary>
        /// Actualiza completamente un usuario existente en todas las bases de datos
        /// </summary>
        /// <param name="id">ID del usuario a actualizar</param>
        /// <param name="userDto">Nuevos datos del usuario</param>
        /// <returns>Confirmación de actualización</returns>
        /// <response code="204">Usuario actualizado exitosamente</response>
        /// <response code="400">Datos inválidos</response>
        /// <response code="404">Usuario no encontrado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateUser(
            [Range(1, int.MaxValue)] int id, 
            [FromBody] UserDto userDto)
        {
            if (userDto == null)
                return BadRequest(new { message = "Los datos del usuario son requeridos" });

            if (id != userDto.Id)
                return BadRequest(new { message = "El ID del usuario no coincide con el del objeto" });

            try
            {
                _logger.LogInformation("Actualizando usuario con ID: {Id}", id);
                await _userBusiness.UpdateUserAsync(userDto);
                
                _logger.LogInformation("Usuario actualizado exitosamente con ID: {Id}", id);
                return NoContent();
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning("Validación fallida al actualizar usuario ID {Id}: {Error}", id, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogWarning("Usuario no encontrado para actualización con ID: {Id}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (ExternalServiceException ex)
            {
                _logger.LogError(ex, "Error de servicio externo al actualizar usuario con ID: {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "Error al acceder a la base de datos" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al actualizar usuario con ID: {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "Error interno del servidor al actualizar usuario" });
            }
        }

        /// <summary>
        /// Actualiza parcialmente un usuario existente en todas las bases de datos
        /// </summary>
        /// <param name="id">ID del usuario a actualizar</param>
        /// <param name="userDto">Datos parciales del usuario</param>
        /// <returns>Confirmación de actualización</returns>
        /// <response code="200">Usuario actualizado exitosamente</response>
        /// <response code="400">Datos inválidos</response>
        /// <response code="404">Usuario no encontrado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPatch("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PatchUser(
            [Range(1, int.MaxValue)] int id, 
            [FromBody] UserDto userDto)
        {
            if (userDto == null)
                return BadRequest(new { message = "Los datos del usuario son requeridos" });

            if (id != userDto.Id)
                return BadRequest(new { message = "El ID del usuario no coincide con el del objeto" });

            try
            {
                _logger.LogInformation("Actualizando parcialmente usuario con ID: {Id}", id);
                await _userBusiness.PatchUserAsync(userDto);
                
                _logger.LogInformation("Usuario actualizado parcialmente con éxito con ID: {Id}", id);
                return Ok(new { message = "Usuario actualizado correctamente" });
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning("Validación fallida al actualizar parcialmente usuario ID {Id}: {Error}", id, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogWarning("Usuario no encontrado para actualización parcial con ID: {Id}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (ExternalServiceException ex)
            {
                _logger.LogError(ex, "Error de servicio externo al actualizar parcialmente usuario con ID: {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "Error al acceder a la base de datos" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al actualizar parcialmente usuario con ID: {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "Error interno del servidor al actualizar usuario" });
            }
        }

        /// <summary>
        /// Desactiva un usuario (soft delete) en todas las bases de datos
        /// </summary>
        /// <param name="id">ID del usuario a desactivar</param>
        /// <returns>Confirmación de desactivación</returns>
        /// <response code="204">Usuario desactivado exitosamente</response>
        /// <response code="400">ID inválido</response>
        /// <response code="404">Usuario no encontrado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpDelete("{id:int}/disable")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DisableUser([Range(1, int.MaxValue)] int id)
        {
            try
            {
                _logger.LogInformation("Desactivando usuario con ID: {Id}", id);
                await _userBusiness.DisableUserAsync(id);
                
                _logger.LogInformation("Usuario desactivado exitosamente con ID: {Id}", id);
                return NoContent();
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning("Validación fallida al desactivar usuario ID {Id}: {Error}", id, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogWarning("Usuario no encontrado para desactivación con ID: {Id}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (ExternalServiceException ex)
            {
                _logger.LogError(ex, "Error de servicio externo al desactivar usuario con ID: {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "Error al acceder a la base de datos" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al desactivar usuario con ID: {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "Error interno del servidor al desactivar usuario" });
            }
        }

        /// <summary>
        /// Elimina permanentemente un usuario de todas las bases de datos
        /// </summary>
        /// <param name="id">ID del usuario a eliminar</param>
        /// <returns>Confirmación de eliminación</returns>
        /// <response code="204">Usuario eliminado exitosamente</response>
        /// <response code="400">ID inválido</response>
        /// <response code="404">Usuario no encontrado</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteUser([Range(1, int.MaxValue)] int id)
        {
            try
            {
                _logger.LogInformation("Eliminando usuario con ID: {Id}", id);
                await _userBusiness.DeleteUserAsync(id);
                
                _logger.LogInformation("Usuario eliminado exitosamente con ID: {Id}", id);
                return NoContent();
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning("Validación fallida al eliminar usuario ID {Id}: {Error}", id, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogWarning("Usuario no encontrado para eliminación con ID: {Id}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (ExternalServiceException ex)
            {
                _logger.LogError(ex, "Error de servicio externo al eliminar usuario con ID: {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "Error al acceder a la base de datos" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al eliminar usuario con ID: {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "Error interno del servidor al eliminar usuario" });
            }
        }

        /// <summary>
        /// Obtiene usuarios filtrados por estado activo
        /// </summary>
        /// <param name="active">Estado del usuario (true = activo, false = inactivo)</param>
        /// <returns>Lista de usuarios filtrados</returns>
        /// <response code="200">Retorna los usuarios filtrados</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("active/{active:bool}")]
        [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsersByStatus(bool active)
        {
            try
            {
                _logger.LogInformation("Obteniendo usuarios con estado activo: {Active}", active);
                var allUsers = await _userBusiness.GetAllUsersAsync();
                var filteredUsers = allUsers.Where(u => u.Active == active);
                
                _logger.LogInformation("Se encontraron {Count} usuarios con estado activo: {Active}", 
                    filteredUsers.Count(), active);
                return Ok(filteredUsers);
            }
            catch (ExternalServiceException ex)
            {
                _logger.LogError(ex, "Error de servicio externo al obtener usuarios por estado");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "Error al acceder a la base de datos" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al obtener usuarios por estado");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Busca usuarios por nombre de usuario
        /// </summary>
        /// <param name="username">Nombre de usuario a buscar (búsqueda parcial)</param>
        /// <returns>Lista de usuarios que coinciden</returns>
        /// <response code="200">Retorna los usuarios encontrados</response>
        /// <response code="400">Parámetro de búsqueda inválido</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("search")]
        [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<UserDto>>> SearchUsersByUsername([FromQuery] string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return BadRequest(new { message = "El parámetro de búsqueda 'username' es requerido" });

            try
            {
                _logger.LogInformation("Buscando usuarios con username que contenga: {Username}", username);
                var allUsers = await _userBusiness.GetAllUsersAsync();
                var matchingUsers = allUsers.Where(u => 
                    u.UserName != null && u.UserName.Contains(username, StringComparison.OrdinalIgnoreCase));
                
                _logger.LogInformation("Se encontraron {Count} usuarios que coinciden con la búsqueda", 
                    matchingUsers.Count());
                return Ok(matchingUsers);
            }
            catch (ExternalServiceException ex)
            {
                _logger.LogError(ex, "Error de servicio externo al buscar usuarios");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "Error al acceder a la base de datos" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al buscar usuarios");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Verifica el estado de salud del controlador
        /// </summary>
        /// <returns>Estado del controlador</returns>
        [HttpGet("health")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [AllowAnonymous] // Permite acceso sin autenticación para health checks
        public IActionResult HealthCheck()
        {
            return Ok(new { status = "healthy", timestamp = DateTime.UtcNow, controller = "User" });
        }
    }
}