using Business;
using Entity.Context;
using Entity.DTOs;
using Entity.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly AuthBusiness _authBusiness;
        private readonly IDbContextFactory<SqlServerDbContext> _contextFactory; // O PostgresDbContext, MySqlDbContext

        public AuthController(AuthBusiness authBusiness, IDbContextFactory<SqlServerDbContext> contextFactory)
        {
            _authBusiness = authBusiness;
            _contextFactory = contextFactory;
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

            using var _context = _contextFactory.CreateDbContext();

            // Validar que no existe el usuario
            var existingUser = await _context.User
                .FirstOrDefaultAsync(u => u.UserName == dto.UserName);
            
            if (existingUser != null)
                return BadRequest(new { message = "El nombre de usuario ya existe" });

            // Validar que no existe el email
            var existingEmail = await _context.Person
                .FirstOrDefaultAsync(p => p.Email == dto.Email);
            
            if (existingEmail != null)
                return BadRequest(new { message = "El email ya está registrado" });

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
                var defaultRolEntity = await _context.Rol
                    .FirstOrDefaultAsync(r => r.Name == "User");
                
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

                return Ok(new { 
                    message = "Usuario registrado correctamente",
                    userName = user.UserName
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = $"Error interno: {ex.Message}" });
            }
        }
    }
}