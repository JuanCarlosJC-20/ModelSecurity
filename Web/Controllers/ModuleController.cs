using Business;
using Entity.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize]
    public class ModuleController : ControllerBase
    {
        private readonly ModuleBusiness _moduleBusiness;
        private readonly ILogger<ModuleController> _logger;

        public ModuleController(ModuleBusiness moduleBusiness, ILogger<ModuleController> logger)
        {
            _moduleBusiness = moduleBusiness;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var modules = await _moduleBusiness.GetAllModuleAsync();
            return Ok(modules);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var module = await _moduleBusiness.GetModuleByIdAsync(id);
            return Ok(module);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ModuleDto dto)
        {
            var created = await _moduleBusiness.CreateModuleAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ModuleDto dto)
        {
            if (id != dto.Id)
                return BadRequest(new { message = "El ID de la URL no coincide con el objeto enviado" });

            var updated = await _moduleBusiness.UpdateModuleAsync(dto);
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _moduleBusiness.DeleteModuleAsync(id);
            return NoContent();
        }

        [HttpDelete("{id}/disable")]
        public async Task<IActionResult> Disable(int id)
        {
            await _moduleBusiness.DisableModuleAsync(id);
            return NoContent();
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> PartialUpdate(int id, [FromBody] ModuleDto dto)
        {
            if (id != dto.Id)
                return BadRequest(new { message = "El ID de la URL no coincide con el objeto enviado" });

            await _moduleBusiness.PartialUpdateModuleAsync(dto);
            return NoContent();
        }
    }
}
