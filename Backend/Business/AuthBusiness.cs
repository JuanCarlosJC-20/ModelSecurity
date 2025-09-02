using Business.Services;
using Data;
using Entity.Context;
using Entity.DTOs;
using Entity.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Business
{
    public class AuthBusiness
    {
        private readonly MultiDatabaseService _multiDbService;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthBusiness> _logger;

        public AuthBusiness(
            MultiDatabaseService multiDbService,
            IConfiguration config,
            ILogger<AuthBusiness> logger)
        {
            _multiDbService = multiDbService;
            _config = config;
            _logger = logger;
        }

        public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto login)
        {
            try
            {
                // Buscar usuario con roles y permisos
                var user = await _multiDbService.ExecuteWithFailoverAsync(async (context) =>
                {
                    return await context.Set<User>()
                        .Include(u => u.Person)
                        .FirstOrDefaultAsync(u => u.UserName == login.UserName && u.Active);
                });

                // Obtener roles y permisos del usuario
                var userRoles = await _multiDbService.ExecuteWithFailoverAsync(async (context) =>
                {
                    return await context.Set<RolUser>()
                        .Include(ru => ru.Rol)
                        .Where(ru => ru.UserId == user.Id && ru.Rol.Active)
                        .Select(ru => ru.Rol.Name)
                        .ToListAsync();
                });

                var userPermissions = await _multiDbService.ExecuteWithFailoverAsync(async (context) =>
                {
                    return await context.Set<RolUser>()
                        .Where(ru => ru.UserId == user.Id && ru.Rol.Active)
                        .SelectMany(ru => context.Set<RolFormPermission>()
                            .Where(rfp => rfp.RolId == ru.RolId && rfp.Permission.Active)
                            .Select(rfp => rfp.Permission.Name))
                        .Distinct()
                        .ToListAsync();
                });

                if (user == null)
                {
                    _logger.LogWarning("Usuario no encontrado: {UserName}", login.UserName);
                    return null;
                }

                if (!BCrypt.Net.BCrypt.Verify(login.Password, user.PasswordHash))
                {
                    _logger.LogWarning("Contraseña incorrecta para usuario: {UserName}", login.UserName);
                    return null;
                }

                // Actualizar último login en todas las bases de datos
                var lastLogin = DateTime.UtcNow;
                await _multiDbService.ExecuteInAllDatabasesAsync(async (context) =>
                {
                    var userToUpdate = await context.Set<User>()
                        .FirstOrDefaultAsync(u => u.Id == user.Id);
                    
                    if (userToUpdate != null)
                    {
                        // Asumo que existe un campo LastLogin, si no existe, puedes omitir esto
                        // userToUpdate.LastLogin = lastLogin;
                        await context.SaveChangesAsync();
                    }
                });

                var token = GenerateJwtToken(user, userRoles ?? new List<string>());
                _logger.LogInformation("Login exitoso para usuario: {UserName}", login.UserName);

                return new LoginResponseDto
                {
                    Token = token,
                    Expiration = DateTime.UtcNow.AddHours(1),
                    UserName = user.UserName,
                    UserId = user.Id,
                    FirstName = user.Person?.FirstName ?? "",
                    LastName = user.Person?.LastName ?? "",
                    Email = user.Person?.Email ?? "",
                    Roles = userRoles ?? new List<string>(),
                    Permissions = userPermissions ?? new List<string>()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el login para usuario: {UserName}", login.UserName);
                return null;
            }
        }

        public async Task<RegisterResponseDto?> RegisterAsync(RegisterRequestDto register)
        {
            try
            {
                // Verificar si el usuario ya existe con failover
                var existingUser = await _multiDbService.ExecuteWithFailoverAsync(async (context) =>
                {
                    return await context.Set<User>()
                        .FirstOrDefaultAsync(u => u.UserName == register.UserName);
                });

                if (existingUser != null)
                {
                    _logger.LogWarning("Usuario ya existe: {UserName}", register.UserName);
                    return null;
                }

                // Verificar si el email ya existe
                var existingEmail = await _multiDbService.ExecuteWithFailoverAsync(async (context) =>
                {
                    return await context.Set<Person>()
                        .FirstOrDefaultAsync(p => p.Email == register.Email);
                });

                if (existingEmail != null)
                {
                    _logger.LogWarning("Email ya registrado: {Email}", register.Email);
                    return null;
                }

                // Crear usuario en todas las bases de datos (sin transacciones manuales)
                var userId = 0;
                await _multiDbService.ExecuteInAllDatabasesAsync(async (context) =>
                {
                    // Crear persona
                    var person = new Person
                    {
                        FirstName = register.FirstName,
                        LastName = register.LastName,
                        Email = register.Email
                    };
                    context.Set<Person>().Add(person);
                    await context.SaveChangesAsync();

                    // Crear usuario
                    var user = new User
                    {
                        UserName = register.UserName,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(register.Password),
                        Code = $"User{person.Id}",
                        PersonId = person.Id,
                        Active = true,
                        CreateAt = DateTime.UtcNow
                    };
                    context.Set<User>().Add(user);
                    await context.SaveChangesAsync();

                    // Asignar rol por defecto
                    var defaultRole = await context.Set<Rol>()
                        .FirstOrDefaultAsync(r => r.Name == "User" && r.Active);

                    if (defaultRole != null)
                    {
                        var roleUser = new RolUser
                        {
                            UserId = user.Id,
                            RolId = defaultRole.Id
                        };
                        context.Set<RolUser>().Add(roleUser);
                        await context.SaveChangesAsync();
                    }
                    
                    if (userId == 0) userId = user.Id;
                });

                _logger.LogInformation("Usuario registrado exitosamente: {UserName}", register.UserName);
                return new RegisterResponseDto
                {
                    Success = true,
                    Message = "Usuario registrado correctamente",
                    UserName = register.UserName
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el registro de usuario: {UserName}", register.UserName);
                return new RegisterResponseDto
                {
                    Success = false,
                    Message = $"Error interno: {ex.Message}"
                };
            }
        }

        private string GenerateJwtToken(User user, List<string> roles)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim("userId", user.Id.ToString()),
                new Claim("firstName", user.Person?.FirstName ?? ""),
                new Claim("lastName", user.Person?.LastName ?? ""),
                new Claim("email", user.Person?.Email ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Agregar roles como claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}