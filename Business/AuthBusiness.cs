using Data;
using Entity.DTOs;
using Entity.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net; // Agregar esta línea

namespace Business
{
    public class AuthBusiness
    {
        private readonly AuthData _authData; // Cambio: usar AuthData en lugar de UserData
        private readonly IConfiguration _config;

        public AuthBusiness(AuthData authData, IConfiguration config) // Cambio aquí también
        {
            _authData = authData;
            _config = config;
        }

        public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto login)
        {
            // Buscar usuario usando AuthData
            var user = await _authData.GetByUserNameAsync(login.UserName);

            if (user == null) return null;

            // ✅ CORRECCIÓN: Verificar contraseña con BCrypt
            if (!BCrypt.Net.BCrypt.Verify(login.Password, user.PasswordHash)) 
                return null;

            // Actualizar último login (opcional)
            await _authData.UpdateLastLoginAsync(user.Id, DateTime.UtcNow);

            // Generar JWT
            var token = GenerateJwtToken(user);

            return new LoginResponseDto
            {
                Token = token,
                Expiration = DateTime.UtcNow.AddHours(1),
                UserName = user.UserName
            };
        }

        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim("userId", user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

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