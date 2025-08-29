using Business;
using Entity.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using Utilities.Exceptions;
using ValidationException = Utilities.Exceptions.ValidationException;

namespace Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class RolUserController : ControllerBase
    {
        private readonly RolUserBusiness _business;
        private readonly ILogger<RolUserController> _logger;

        public RolUserController(RolUserBusiness business, ILogger<RolUserController> logger)
        {
            _business = business;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<RolUserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllRolUsers()
        {
            try
            {
                var rolUsers = await _business.GetAllRolUsersAsync();
                return Ok(rolUsers);
            }
            catch (ExternalServiceException ex)
            {
                _logger.LogError(ex, "Error al obtener RolUsers desde la base de datos");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Error al acceder a la base de datos", detail = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al obtener todos los RolUsers");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Error interno del servidor" });
            }
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(RolUserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRolUserById([Range(1, int.MaxValue)] int id)
        {
            try
            {
                var rolUser = await _business.GetRolUserByIdAsync(id);
                return Ok(rolUser);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (EntityNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ExternalServiceException ex)
            {
                _logger.LogError(ex, "Error al obtener RolUser con ID: {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Error al acceder a la base de datos", detail = ex.Message });
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(RolUserDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateRolUser([FromBody] RolUserDto rolUserDto)
        {
            try
            {
                var created = await _business.CreateRolUserAsync(rolUserDto);
                return CreatedAtAction(nameof(GetRolUserById), new { id = created.Id }, created);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ExternalServiceException ex)
            {
                _logger.LogError(ex, "Error al crear RolUser");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(RolUserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateRolUser(int id, [FromBody] RolUserDto rolUserDto)
        {
            try
            {
                rolUserDto.Id = id;
                var updated = await _business.UpdateRolUserAsync(id, rolUserDto);
                return Ok(updated);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (EntityNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ExternalServiceException ex)
            {
                _logger.LogError(ex, "Error al actualizar RolUser con ID: {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteRolUser(int id)
        {
            try
            {
                await _business.DeleteRolUserAsync(id);
                return NoContent();
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (EntityNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ExternalServiceException ex)
            {
                _logger.LogError(ex, "Error al eliminar RolUser con ID: {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = ex.Message });
            }
        }

        // Aquí podrías tener métodos dedicados en Business en lugar de filtrar en memoria
        [HttpGet("user/{userId:int}")]
        public async Task<IActionResult> GetRolUsersByUserId(int userId)
        {
            var result = await _business.GetRolUsersByUserIdAsync(userId);
            return Ok(result);
        }

        [HttpGet("role/{rolId:int}")]
        public async Task<IActionResult> GetRolUsersByRolId(int rolId)
        {
            var result = await _business.GetRolUsersByRolIdAsync(rolId);
            return Ok(result);
        }

        [HttpGet("health")]
        public IActionResult HealthCheck()
        {
            return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
        }
    }
}
