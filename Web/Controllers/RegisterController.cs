using Business;
using Entity.Context;
using Entity.DTOs;
using Entity.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using BCrypt.Net;

namespace Web.Controllers
{   
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            // 1. Crear la persona
            var person = new Person
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email
            };

            _context.Person.Add(person);
            await _context.SaveChangesAsync();

            // 2. Crear el usuario con la contraseña hasheada
            var user = new User
            {
                UserName = dto.UserName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Code = $"User{person.Id}",
                PersonId = person.Id,
                Active = true,
                CreateAt = DateTime.UtcNow
            };

            _context.User.Add(user);
            await _context.SaveChangesAsync();

            // 3. Asignar rol por defecto
            var defaultRol = new RolUser
            {
                UserId = user.Id,
                RolId = 1 // Cambia si necesitas otro rol por defecto
            };

            _context.RolUser.Add(defaultRol);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Usuario registrado correctamente" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            var user = _context.User.FirstOrDefault(u => u.UserName == dto.Username && u.Active);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Credenciales inválidas" });
            }

            return Ok(new { message = "Inicio de sesión exitoso" });
        }
    }
}
