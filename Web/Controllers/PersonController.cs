using Business;
using Data;
using Entity.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization; 

using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utilities.Exceptions;

namespace Web.ContUserlers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    [Authorize] //  Ahora TODOS los endpoints de este controlador requieren token JWT
    public class PersonController : ControllerBase
    {
        private readonly PersonBusiness _PersonBusiness;
        private readonly ILogger<PersonController> _logger;

        public PersonController(PersonBusiness PersonBusiness, ILogger<PersonController> logger)
        {
            _PersonBusiness = PersonBusiness;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PersonDto>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAllPerson()
        {
            try
            {
                var Person = await _PersonBusiness.GetAllPersonAsync();
                return Ok(Person);
            }
            catch (ExternalServiceException ex)
            {
                _logger.LogError(ex, "Error al obtener personas");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PersonDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetPersonById(int id)
        {
            try
            {
                var Person = await _PersonBusiness.GetPersonByIdAsync(id);
                return Ok(Person);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validación fallida para la persona con ID: {PersonId}", id);
                return BadRequest(new { message = ex.Message });
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogInformation(ex, "Persona no encontrada con ID: {PersonId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (ExternalServiceException ex)
            {
                _logger.LogError(ex, "Error al obtener persona con ID: {PersonId}", id);
                return StatusCode(500, new { message = ex.Message });
            }
        }



        [HttpPost]
        [ProducesResponseType(typeof(PersonDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CreatePerson([FromBody] PersonDto PersonDto)
        {
            try
            {
                var createdPerson = await _PersonBusiness.CreatePersonAsync(PersonDto);
                return CreatedAtAction(nameof(GetPersonById), new { id = createdPerson.Id }, createdPerson);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validación fallida al crear persona");
                return BadRequest(new { message = ex.Message });
            }
            catch (ExternalServiceException ex)
            {
                _logger.LogError(ex, "Error al crear persona");
                return StatusCode(500, new { message = ex.Message });
            }
        }



        [HttpPut("{id}")]
        [ProducesResponseType(typeof(PersonDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> UpdatePerson(int id, [FromBody] PersonDto personDto)
        {
            try
            {
                var updatedPerson = await _PersonBusiness.UpdatePersonAsync(id, personDto);
                return Ok(updatedPerson);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validación fallida al actualizar persona con ID: {PersonId}", id);
                return BadRequest(new { message = ex.Message });
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogInformation(ex, "Persona no encontrada con ID: {PersonId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (ExternalServiceException ex)
            {
                _logger.LogError(ex, "Error al actualizar persona con ID: {PersonId}", id);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DeletePerson(int id)
        {
            try
            {
                await _PersonBusiness.DeletePersonAsync(id);
                return NoContent();
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validación fallida al eliminar persona con ID: {PersonId}", id);
                return BadRequest(new { message = ex.Message });
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogInformation(ex, "Persona no encontrada con ID: {PersonId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (ExternalServiceException ex)
            {
                _logger.LogError(ex, "Error al eliminar persona con ID: {PersonId}", id);
                return StatusCode(500, new { message = ex.Message });
            }
        }



        ///<summary>
        /// <summary>
        /// Desactiva un formulario (eliminación lógica)
        /// </summary>
        /// <param name="id">ID del formulario a desactivar</param>
        /// <returns>NoContent si fue exitoso</returns>
        [HttpDelete("{id}/disable")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DisableForm(int id)
        {
            try
            {
                await _PersonBusiness.DisableFormAsync(id);
                return NoContent();
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogInformation(ex, "Formulario no encontrado para desactivación con ID: {FormId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (ExternalServiceException ex)
            {
                _logger.LogError(ex, "Error al desactivar formulario con ID: {FormId}", id);
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
public async Task<IActionResult> PartialUpdateForm(int id, [FromBody] PersonDto personDto)
{
    if (id != personDto.Id)
    {
        return BadRequest(new { message = "El ID del permiso no coincide con el del objeto." });
    }

    try
    {
        await _PersonBusiness.PartialUpdateFormAsync(personDto);
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
