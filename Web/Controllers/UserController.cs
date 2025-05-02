using Business;
using Data;
using Entity.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utilities.Exceptions;

namespace Web.ContUserlers
{
    /// <summary>
    /// Controlador para la gestión de usuarios en el sistema
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class UserController : ControllerBase
    {
        private readonly UserBusiness _UserBusiness;
        private readonly ILogger<UserController> _logger;

        public UserController(UserBusiness UserBusiness, ILogger<UserController> logger)
        {
            _UserBusiness = UserBusiness;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<UserDto>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _UserBusiness.GetAllUsersAsync();
                return Ok(users);
            }
            catch (ExternalServiceException ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UserDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetUserById(int id)
        {
            try
            {
                var user = await _UserBusiness.GetUserByIdAsync(id);
                return Ok(user);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validación fallida para el usuario con ID: {UserId}", id);
                return BadRequest(new { message = ex.Message });
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogInformation(ex, "Usuario no encontrado con ID: {UserId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (ExternalServiceException ex)
            {
                _logger.LogError(ex, "Error al obtener usuario con ID: {UserId}", id);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(UserDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CreateUser([FromBody] UserDto userDto)
        {
            try
            {
                var createdUser = await _UserBusiness.CreateUserAsync(userDto);
                return CreatedAtAction(nameof(GetUserById), new { id = createdUser.Id }, createdUser);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validación fallida al crear usuario");
                return BadRequest(new { message = ex.Message });
            }
            catch (ExternalServiceException ex)
            {
                _logger.LogError(ex, "Error al crear usuario");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza un usuario existente
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(UserDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserDto userDto)
        {
            try
            {
                userDto.Id = id;
                var updatedUser = await _UserBusiness.UpdateUserAsync(id, userDto);
                return Ok(updatedUser);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validación fallida al actualizar usuario");
                return BadRequest(new { message = ex.Message });
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogInformation(ex, "Usuario no encontrado para actualizar con ID: {UserId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (ExternalServiceException ex)
            {
                _logger.LogError(ex, "Error al actualizar usuario con ID: {UserId}", id);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Elimina un usuario por ID
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                await _UserBusiness.DeleteUserAsync(id);
                return NoContent();
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validación fallida al eliminar usuario");
                return BadRequest(new { message = ex.Message });
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogInformation(ex, "Usuario no encontrado para eliminar con ID: {UserId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (ExternalServiceException ex)
            {
                _logger.LogError(ex, "Error al eliminar usuario con ID: {UserId}", id);
                return StatusCode(500, new { message = ex.Message });
            }
        }
  ///<summary>
        /// <summary>
        /// Desactiva un rol (eliminación lógica)
        /// </summary>
        /// <param name="id">ID del rol a desactivar</param>
        /// <returns>NoContent si fue exitoso</returns>
        [HttpDelete("{id}/disable")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Disablerol(int id)
        {
            try
            {
                await _UserBusiness.DisableFormAsync(id);
                return NoContent();
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogInformation(ex, "rol no encontrado para desactivación con ID: {rolId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (ExternalServiceException ex)
            {
                _logger.LogError(ex, "Error al desactivar rol con ID: {FormId}", id);
                return StatusCode(500, new { message = ex.Message });
            }
        }


        
         /// <summary>
/// Actualiza parcialmente un permiso existente.
/// </summary>
/// <param name="id">ID del permiso a actualizar.</param>
/// <param name="formDto">Datos parciales del permiso.</param>
/// <returns>Resultado de la operación.</returns>
[HttpPatch("{id}")]
[ProducesResponseType(204)]
[ProducesResponseType(400)]
[ProducesResponseType(404)]
[ProducesResponseType(500)]
public async Task<IActionResult> PartialUpdateForm(int id, [FromBody] UserDto userDto)
{
    if (id != userDto.Id)
    {
        return BadRequest(new { message = "El ID del permiso no coincide con el del objeto." });
    }

    try
    {
        await _UserBusiness.PartialUpdateFormAsync(userDto);
        return NoContent(); // 204: Actualizado correctamente sin contenido de respuesta
    }
    catch (ValidationException ex)
    {
        _logger.LogWarning(ex, "Validación fallida al actualizar parcialmente el permiso con ID: {FormId}", id);
        return BadRequest(new { message = ex.Message });
    }
    catch (EntityNotFoundException ex)
    {
        _logger.LogInformation(ex, "Permiso no encontrado para actualización parcial. ID: {FormId}", id);
        return NotFound(new { message = ex.Message });
    }
    catch (ExternalServiceException ex)
    {
        _logger.LogError(ex, "Error de servicio externo al actualizar permiso parcialmente con ID: {FormId}", id);
        return StatusCode(500, new { message = ex.Message });
    }
}

    }
}
