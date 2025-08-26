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
    public class FormModuleController : ControllerBase
    {
        private readonly FormModuleBusiness _formModuleBusiness;
        private readonly ILogger<FormModuleController> _logger;

        public FormModuleController(FormModuleBusiness formModuleBusiness, ILogger<FormModuleController> logger)
        {
            _formModuleBusiness = formModuleBusiness;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var formModules = await _formModuleBusiness.GetAllFormModulesAsync();
            return Ok(formModules);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var formModule = await _formModuleBusiness.GetFormModuleByIdAsync(id);
            return Ok(formModule);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] FormModuleDto dto)
        {
            var created = await _formModuleBusiness.CreateFormModuleAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] FormModuleDto dto)
        {
            if (id != dto.Id)
                return BadRequest(new { message = "ID de la URL no coincide con el objeto." });

            await _formModuleBusiness.UpdateFormModuleAsync(dto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _formModuleBusiness.DeleteFormModuleAsync(id);
            return NoContent();
        }
    }
}
