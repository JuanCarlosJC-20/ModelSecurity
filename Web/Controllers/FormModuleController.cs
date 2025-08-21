using Business;
using Entity.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utilities.Exceptions;

namespace Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    [Authorize] // 👈 Ahora TODOS los endpoints de este controlador requieren token JWT
    public class FormModuleController : ControllerBase
    {
        private readonly FormModuleBusiness _FormModuleBusiness;
        private readonly ILogger<FormModuleController> _logger;

        public FormModuleController(FormModuleBusiness FormModuleBusiness, ILogger<FormModuleController> logger)
        {
            _FormModuleBusiness = FormModuleBusiness;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<FormModuleDto>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAllFormModules()
        {
            try
            {
                var FormModules = await _FormModuleBusiness.GetAllFormModulesAsync();
                return Ok(FormModules);
            }
            catch (ExternalServiceException ex)
            {
                _logger.LogError(ex, "Error al obtener permisos");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(FormModuleDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetFormModuleById(int id)
        {
            try
            {
                var FormModule = await _FormModuleBusiness.GetFormModuleByIdAsync(id);
                return Ok(FormModule);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validación fallida para el permiso con ID: {FormModuleId}", id);
                return BadRequest(new { message = ex.Message });
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogInformation(ex, "Permiso no encontrado con ID: {FormModuleId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (ExternalServiceException ex)
            {
                _logger.LogError(ex, "Error al obtener permiso con ID: {FormModuleId}", id);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(FormModuleDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CreateFormModule([FromBody] FormModuleDto FormModuleDto)
        {
            try
            {
                var createdFormModule = await _FormModuleBusiness.CreateFormModuleAsync(FormModuleDto);
                return CreatedAtAction(nameof(GetFormModuleById), new { id = createdFormModule.Id }, createdFormModule);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validación fallida al crear permiso");
                return BadRequest(new { message = ex.Message });
            }
            catch (ExternalServiceException ex)
            {
                _logger.LogError(ex, "Error al crear permiso");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> UpdateFormModule(int id, [FromBody] FormModuleDto formModuleDto)
        {
            try
            {
                if (id != formModuleDto.Id)
                    return BadRequest(new { message = "El ID de la URL no coincide con el del cuerpo de la solicitud." });

                await _FormModuleBusiness.UpdateFormModuleAsync(formModuleDto);
                return NoContent();
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validación fallida al actualizar permiso");
                return BadRequest(new { message = ex.Message });
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogInformation(ex, "Formulario-Módulo no encontrado con ID: {FormModuleId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (ExternalServiceException ex)
            {
                _logger.LogError(ex, "Error al actualizar permiso");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DeleteFormModule(int id)
        {
            try
            {
                await _FormModuleBusiness.DeleteFormModuleAsync(id);
                return NoContent();
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "ID inválido para eliminar permiso");
                return BadRequest(new { message = ex.Message });
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogInformation(ex, "Formulario-Módulo no encontrado con ID: {FormModuleId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (ExternalServiceException ex)
            {
                _logger.LogError(ex, "Error al eliminar permiso");
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
