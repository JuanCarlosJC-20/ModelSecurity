using Business;
using Business.Services;
using Entity.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly AuthBusiness _authBusiness;
        private readonly MultiDatabaseService _multiDbService;

        public AuthController(AuthBusiness authBusiness, MultiDatabaseService multiDbService)
        {
            _authBusiness = authBusiness;
            _multiDbService = multiDbService;
        }

        // ==========================
        // LOGIN
        // ==========================
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto login)
        {
            if (login == null || string.IsNullOrEmpty(login.UserName) || string.IsNullOrEmpty(login.Password))
                return BadRequest(new { message = "Username y password son requeridos" });

            var result = await _authBusiness.LoginAsync(login);

            if (result == null)
                return Unauthorized(new { message = "Credenciales inválidas" });

            return Ok(result);
        }

        // ==========================
        // REGISTRO DE USUARIO
        // ==========================
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (dto == null)
                return BadRequest(new { message = "Datos de registro requeridos" });

            var registerRequest = new RegisterRequestDto
            {
                UserName = dto.UserName,
                Password = dto.Password,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email
            };

            var result = await _authBusiness.RegisterAsync(registerRequest);

            if (result == null)
                return BadRequest(new { message = "El usuario ya existe o el email ya está registrado" });

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { 
                message = result.Message,
                userName = result.UserName
            });
        }

        // ==========================
        // ESTADO DE BASES DE DATOS
        // ==========================
        [HttpGet("database-status")]
        public async Task<IActionResult> GetDatabaseStatus()
        {
            try
            {
                var status = await _multiDbService.GetDatabaseStatusAsync();
                var healthyCount = status.Count(s => s.Value);
                var totalCount = status.Count;

                return Ok(new 
                { 
                    healthy = healthyCount,
                    total = totalCount,
                    status = "OK",
                    databases = status
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new 
                { 
                    status = "ERROR",
                    message = ex.Message
                });
            }
        }
    }
}