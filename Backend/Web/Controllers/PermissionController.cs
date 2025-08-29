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
    public class PermissionController : ControllerBase
    {
        private readonly PermissionBusiness _permissionBusiness;
        private readonly ILogger<PermissionController> _logger;

        public PermissionController(PermissionBusiness permissionBusiness, ILogger<PermissionController> logger)
        {
            _permissionBusiness = permissionBusiness;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var permissions = await _permissionBusiness.GetAllPermissionAsync();
            return Ok(permissions);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var permission = await _permissionBusiness.GetPermissionByIdAsync(id);
            return Ok(permission);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PermissionDto dto)
        {
            var created = await _permissionBusiness.CreatePermissionAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] PermissionDto dto)
        {
            if (id != dto.Id)
                return BadRequest(new { message = "El ID de la URL no coincide con el objeto enviado" });

            var updated = await _permissionBusiness.UpdatePermissionAsync(dto);
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _permissionBusiness.DeletePermissionAsync(id);
            return NoContent();
        }

        [HttpDelete("{id}/disable")]
        public async Task<IActionResult> Disable(int id)
        {
            await _permissionBusiness.DisablePermissionAsync(id);
            return NoContent();
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> PartialUpdate(int id, [FromBody] PermissionDto dto)
        {
            if (id != dto.Id)
                return BadRequest(new { message = "El ID de la URL no coincide con el objeto enviado" });

            await _permissionBusiness.PartialUpdatePermissionAsync(dto);
            return NoContent();
        }
    }
}
