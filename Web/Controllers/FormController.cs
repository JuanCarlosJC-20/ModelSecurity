using Business;
using Entity.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Utilities.Exceptions;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize]
    public class FormController : ControllerBase
    {
        private readonly FormBusiness _formBusiness;
        private readonly ILogger<FormController> _logger;

        public FormController(FormBusiness formBusiness, ILogger<FormController> logger)
        {
            _formBusiness = formBusiness;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var forms = await _formBusiness.GetAllFormsAsync();
            return Ok(forms);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var form = await _formBusiness.GetFormByIdAsync(id);
            return Ok(form);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] FormDto dto)
        {
            var created = await _formBusiness.CreateFormAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] FormDto dto)
        {
            if (id != dto.Id)
                return BadRequest(new { message = "ID de la URL no coincide con el objeto." });

            await _formBusiness.UpdateFormAsync(dto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _formBusiness.DeleteFormAsync(id);
            return NoContent();
        }

        [HttpDelete("{id}/disable")]
        public async Task<IActionResult> Disable(int id)
        {
            await _formBusiness.DisableFormAsync(id);
            return NoContent();
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> PartialUpdate(int id, [FromBody] FormDto dto)
        {
            if (id != dto.Id)
                return BadRequest(new { message = "ID de la URL no coincide con el objeto." });

            await _formBusiness.PartialUpdateFormAsync(dto);
            return NoContent();
        }
    }
}
