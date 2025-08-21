using Business;
using Entity.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Utilities.Exceptions;

namespace Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize] // Ahora TODOS los endpoints de este controlador requieren token JWT
    public class UserController : ControllerBase
    {
        private readonly UserBusiness _userBusiness;
        private readonly ILogger<UserController> _logger;

        public UserController(UserBusiness userBusiness, ILogger<UserController> logger)
        {
            _userBusiness = userBusiness;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<UserDto>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _userBusiness.GetAllUsersAsync();
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
                var user = await _userBusiness.GetUserByIdAsync(id);
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

        // [HttpPost]
        // [ProducesResponseType(typeof(UserDto), 201)]
        // [ProducesResponseType(400)]
        // [ProducesResponseType(500)]
        // public async Task<IActionResult> CreateUser([FromBody] UserDto userDto)
        // {
        //     try
        //     {
        //         var createdUser = await _userBusiness.CreateUserAsync(userDto);
        //         return CreatedAtAction(nameof(GetUserById), new { id = createdUser.Id }, createdUser);
        //     }
        //     catch (ValidationException ex)
        //     {
        //         _logger.LogWarning(ex, "Validación fallida al crear usuario");
        //         return BadRequest(new { message = ex.Message });
        //     }
        //     catch (ExternalServiceException ex)
        //     {
        //         _logger.LogError(ex, "Error al crear usuario");
        //         return StatusCode(500, new { message = ex.Message });
        //     }
        // }

        [HttpPut("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserDto userDto)
        {
            if (id != userDto.Id)
                return BadRequest(new { message = "El ID del usuario no coincide con el del objeto." });

            try
            {
                await _userBusiness.UpdateUserAsync(userDto);
                return NoContent();
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validación fallida al actualizar usuario con ID: {UserId}", id);
                return BadRequest(new { message = ex.Message });
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogInformation(ex, "Usuario no encontrado con ID: {UserId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (ExternalServiceException ex)
            {
                _logger.LogError(ex, "Error al actualizar usuario con ID: {UserId}", id);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPatch("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> PatchUser(int id, [FromBody] UserDto userDto)
        {
            if (id != userDto.Id)
                return BadRequest(new { message = "El ID del usuario no coincide con el del objeto." });

            try
            {
                await _userBusiness.PatchUserAsync(userDto);
                return Ok(new { message = "Usuario actualizado correctamente" });
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validación fallida al actualizar parcialmente el usuario con ID: {UserId}", id);
                return BadRequest(new { message = ex.Message });
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogInformation(ex, "Usuario no encontrado con ID: {UserId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (ExternalServiceException ex)
            {
                _logger.LogError(ex, "Error al actualizar parcialmente el usuario con ID: {UserId}", id);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("{id}/disable")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DisableUser(int id)
        {
            try
            {
                await _userBusiness.DisableUserAsync(id);
                return NoContent();
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogInformation(ex, "Usuario no encontrado para desactivación con ID: {UserId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (ExternalServiceException ex)
            {
                _logger.LogError(ex, "Error al desactivar usuario con ID: {UserId}", id);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                await _userBusiness.DeleteUserAsync(id);
                return NoContent();
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogInformation(ex, "Usuario no encontrado para eliminación con ID: {UserId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (ExternalServiceException ex)
            {
                _logger.LogError(ex, "Error al eliminar usuario con ID: {UserId}", id);
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
