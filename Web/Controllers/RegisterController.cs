using Business;
using Entity.Context;
using Entity.DTOs;
using Entity.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using System.Linq;

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

        // ==========================
        // REGISTRO DE USUARIO
        // ==========================
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
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

                // 2. Crear el usuario con contraseña hasheada
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

                // 3. Obtener rol por defecto
                var defaultRolEntity = await _context.Rol.FirstOrDefaultAsync(r => r.Name == "Admin");
                if (defaultRolEntity == null)
                {
                    return BadRequest(new { message = "No se encontró el rol por defecto 'Admin'." });
                }

                // 4. Asignar rol al usuario
                var defaultRol = new RolUser
                {
                    UserId = user.Id,
                    RolId = defaultRolEntity.Id
                };
                _context.RolUser.Add(defaultRol);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new { message = "Usuario registrado correctamente" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ==========================
        // LOGIN DE USUARIO
        // ==========================
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            var user = await _context.User
                .FirstOrDefaultAsync(u => u.UserName == dto.UserName && u.Active);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Credenciales inválidas" });
            }

            // Aquí puedes generar JWT si quieres
            return Ok(new { message = "Inicio de sesión exitoso" });
        }
    }
}
