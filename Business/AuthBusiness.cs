using Data;
using Entity.Context;
using Entity.DTOs;
using Entity.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Business
{
    public class AuthBusiness
    {
        private readonly AuthData<SqlServerDbContext> _sqlAuthData;
        private readonly AuthData<PostgresDbContext> _pgAuthData;
        private readonly AuthData<MySqlDbContext> _myAuthData;
        private readonly IConfiguration _config;

        public AuthBusiness(
            AuthData<SqlServerDbContext> sqlAuthData,
            AuthData<PostgresDbContext> pgAuthData,
            AuthData<MySqlDbContext> myAuthData,
            IConfiguration config)
        {
            _sqlAuthData = sqlAuthData;
            _pgAuthData = pgAuthData;
            _myAuthData = myAuthData;
            _config = config;
        }

        public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto login)
        {
            // Intentar login en SQL Server primero
            var user = await _sqlAuthData.GetByUserNameAsync(login.UserName);

            // Si no se encuentra, intentar en PostgreSQL
            if (user == null)
                user = await _pgAuthData.GetByUserNameAsync(login.UserName);

            // Si aún no se encuentra, intentar en MySQL
            if (user == null)
                user = await _myAuthData.GetByUserNameAsync(login.UserName);

            if (user == null) return null;

            if (!BCrypt.Net.BCrypt.Verify(login.Password, user.PasswordHash))
                return null;

            // Actualizar último login en todas las bases de datos
            var lastLogin = DateTime.UtcNow;
            await _sqlAuthData.UpdateLastLoginAsync(user.Id, lastLogin);
            await _pgAuthData.UpdateLastLoginAsync(user.Id, lastLogin);
            await _myAuthData.UpdateLastLoginAsync(user.Id, lastLogin);

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