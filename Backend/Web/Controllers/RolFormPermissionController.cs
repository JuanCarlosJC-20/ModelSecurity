using Business;
using Entity.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utilities.Exceptions;

namespace Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class RolFormPermissionController : ControllerBase
    {
        private readonly RolFormPermissionBusiness _business;
        private readonly ILogger<RolFormPermissionController> _logger;

        public RolFormPermissionController(
            RolFormPermissionBusiness business, 
            ILogger<RolFormPermissionController> logger)
        {
            _business = business;
            _logger = logger;
        }

        /// <summary>
        /// Obtener todos los RolFormPermissions
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<RolFormPermissionDto>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _business.GetAllRolFormPermissionsAsync();
                return Ok(result);
            }
            catch (ExternalServiceException ex)
            {
                _logger.LogError(ex, "Error al obtener RolFormPermissions");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Obtener un RolFormPermission por ID
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(RolFormPermissionDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _business.GetRolFormPermissionByIdAsync(id);
                return Ok(result);
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogInformation(ex, "RolFormPermission no encontrado con ID: {Id}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validación fallida para RolFormPermission con ID: {Id}", id);
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Crear un nuevo RolFormPermission
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(RolFormPermissionDto), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Create([FromBody] RolFormPermissionDto dto)
        {
            try
            {
                var created = await _business.CreateRolFormPermissionAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validación fallida al crear RolFormPermission");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Actualizar un RolFormPermission
        /// </summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(RolFormPermissionDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Update(int id, [FromBody] RolFormPermissionDto dto)
        {
            try
            {
                var updated = await _business.UpdateRolFormPermissionAsync(id, dto);
                return Ok(updated);
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogInformation(ex, "RolFormPermission no encontrado con ID: {Id}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validación fallida al actualizar RolFormPermission con ID: {Id}", id);
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Eliminar un RolFormPermission
        /// </summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _business.DeleteRolFormPermissionAsync(id);
                return NoContent();
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogInformation(ex, "RolFormPermission no encontrado con ID: {Id}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validación fallida al eliminar RolFormPermission con ID: {Id}", id);
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
