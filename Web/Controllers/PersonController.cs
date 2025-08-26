using Business;
using Entity.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization; 
using Microsoft.Extensions.Logging;
using Utilities.Exceptions;

namespace Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    [Authorize] // Todos los endpoints requieren token JWT
    public class PersonController : ControllerBase
    {
        private readonly PersonBusiness _personBusiness;
        private readonly ILogger<PersonController> _logger;

        public PersonController(PersonBusiness personBusiness, ILogger<PersonController> logger)
        {
            _personBusiness = personBusiness;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PersonDto>), 200)]
        public async Task<IActionResult> GetAllPerson()
        {
            try
            {
                var persons = await _personBusiness.GetAllPersonAsync();
                return Ok(persons);
            }
            catch (ExternalServiceException ex)
            {
                _logger.LogError(ex, "Error al obtener personas");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PersonDto), 200)]
        public async Task<IActionResult> GetPersonById(int id)
        {
            try
            {
                var person = await _personBusiness.GetPersonByIdAsync(id);
                return Ok(person);
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
                _logger.LogError(ex, "Error al obtener persona con ID {PersonId}", id);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(PersonDto), 201)]
        public async Task<IActionResult> CreatePerson([FromBody] PersonDto personDto)
        {
            try
            {
                var createdPerson = await _personBusiness.CreatePersonAsync(personDto);
                return CreatedAtAction(nameof(GetPersonById), new { id = createdPerson.Id }, createdPerson);
            }
            catch (ValidationException ex)
            {
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
        public async Task<IActionResult> UpdatePerson(int id, [FromBody] PersonDto personDto)
        {
            if (id != personDto.Id)
                return BadRequest(new { message = "El ID de la URL no coincide con el objeto enviado" });

            try
            {
                var updatedPerson = await _personBusiness.UpdatePersonAsync(id, personDto);
                return Ok(updatedPerson);
            }
            catch (EntityNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ExternalServiceException ex)
            {
                _logger.LogError(ex, "Error al actualizar persona con ID {PersonId}", id);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        public async Task<IActionResult> DeletePerson(int id)
        {
            try
            {
                await _personBusiness.DeletePersonAsync(id);
                return NoContent();
            }
            catch (EntityNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ExternalServiceException ex)
            {
                _logger.LogError(ex, "Error al eliminar persona con ID {PersonId}", id);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza parcialmente una persona.
        /// </summary>
        [HttpPatch("{id}")]
        [ProducesResponseType(204)]
        public async Task<IActionResult> PartialUpdatePerson(int id, [FromBody] PersonDto personDto)
        {
            if (id != personDto.Id)
                return BadRequest(new { message = "El ID de la URL no coincide con el objeto enviado" });

            try
            {
                await _personBusiness.PartialUpdatePersonAsync(personDto);
                return NoContent();
            }
            catch (EntityNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ExternalServiceException ex)
            {
                _logger.LogError(ex, "Error al actualizar parcialmente la persona con ID {PersonId}", id);
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
