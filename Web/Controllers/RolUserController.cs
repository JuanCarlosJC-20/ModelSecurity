using Business;
using Entity.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utilities.Exceptions;

namespace Web.ContUserlers
{
    /// <summary>
    /// Controlador para la gestión de relaciones entre usuarios y roles
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    [Authorize] // Ahora TODOS los endpoints de este controlador requieren token JWT
    public class RolUserController : ControllerBase
    {
        private readonly RolUserBusiness _RolUserBusiness;
        private readonly ILogger<RolUserController> _logger;

        public RolUserController(RolUserBusiness RolUserBusiness, ILogger<RolUserController> logger)
        {
            _RolUserBusiness = RolUserBusiness;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todas las relaciones Rol-Usuario
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<RolUserDto>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAllRolUser()
        {
            try
            {
                var RolUser = await _RolUserBusiness.GetAllRolUsersAsync();
                return Ok(RolUser);
            }
            catch (ExternalServiceException ex)
            {
                _logger.LogError(ex, "Error al obtener RolUsers");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene una relación Rol-Usuario por ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(RolUserDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetRolUserById(int id)
        {
            try
            {
                var RolUser = await _RolUserBusiness.GetRolUserByIdAsync(id);
                return Ok(RolUser);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "ID inválido: {id}", id);
                return BadRequest(new { message = ex.Message });
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogInformation(ex, "RolUser no encontrado: {id}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (ExternalServiceException ex)
            {
                _logger.LogError(ex, "Error al obtener RolUser: {id}", id);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Crea una nueva relación Rol-Usuario
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(RolUserDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CreateRolUser([FromBody] RolUserDto RolUserDto)
        {
            try
            {
                var createdRolUser = await _RolUserBusiness.CreateRolUserAsync(RolUserDto);
                return CreatedAtAction(nameof(GetRolUserById), new { id = createdRolUser.Id }, createdRolUser);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validación fallida al crear RolUser");
                return BadRequest(new { message = ex.Message });
            }
            catch (ExternalServiceException ex)
            {
                _logger.LogError(ex, "Error al crear RolUser");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza una relación Rol-Usuario existente
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(RolUserDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> UpdateRolUser(int id, [FromBody] RolUserDto RolUserDto)
        {
            try
            {
                RolUserDto.Id = id; // Aseguramos que el ID del cuerpo coincida con el de la URL
                var updated = await _RolUserBusiness.UpdateRolUserAsync(id, RolUserDto);
                return Ok(updated);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validación fallida al actualizar RolUser");
                return BadRequest(new { message = ex.Message });
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogInformation(ex, "RolUser no encontrado para actualizar: {id}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (ExternalServiceException ex)
            {
                _logger.LogError(ex, "Error al actualizar RolUser: {id}", id);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Elimina una relación Rol-Usuario
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DeleteRolUser(int id)
        {
            try
            {
                await _RolUserBusiness.DeleteRolUserAsync(id);
                return NoContent();
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validación fallida al eliminar RolUser");
                return BadRequest(new { message = ex.Message });
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogInformation(ex, "RolUser no encontrado para eliminar: {id}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (ExternalServiceException ex)
            {
                _logger.LogError(ex, "Error al eliminar RolUser: {id}", id);
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
