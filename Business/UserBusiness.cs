using Data;
using Entity.Context;
using Entity.DTOs;
using Entity.Model;
using Microsoft.Extensions.Logging;
using Utilities.Exceptions;

namespace Business
{
    public class UserBusiness
    {
        private readonly UserData<SqlServerDbContext> _sqlData;
        private readonly UserData<PostgresDbContext> _pgData;
        private readonly UserData<MySqlDbContext> _myData;
        private readonly ILogger<UserBusiness> _logger;

        public UserBusiness(
            UserData<SqlServerDbContext> sqlData,
            UserData<PostgresDbContext> pgData,
            UserData<MySqlDbContext> myData,
            ILogger<UserBusiness> logger)
        {
            _sqlData = sqlData;
            _pgData = pgData;
            _myData = myData;
            _logger = logger;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            try
            {
                var users = await _sqlData.GetAllAsync();
                return users.Select(MapToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los usuarios");
                throw new ExternalServiceException("Base de datos", "Error al recuperar la lista de usuarios", ex);
            }
        }

        public async Task<UserDto> GetUserByIdAsync(int id)
        {
            if (id <= 0)
                throw new ValidationException("id", "El ID del usuario debe ser mayor que cero");

            var user = await _sqlData.GetByIdAsync(id);
            if (user == null)
                throw new EntityNotFoundException("Usuario", id);

            return MapToDto(user);
        }

        public async Task<UserDto> CreateUserAsync(UserDto userDto)
        {
            ValidateUser(userDto);

            var entity = MapToEntity(userDto);
            entity.CreateAt =DateTime.UtcNow;

            try
            {
                var created = await _sqlData.CreateAsync(entity);
                await _pgData.CreateAsync(entity);
                await _myData.CreateAsync(entity);

                return MapToDto(created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario: {UserName}", userDto?.UserName ?? "null");
                throw;
            }
        }

        public async Task UpdateUserAsync(UserDto userDto)
        {
            if (userDto == null || userDto.Id <= 0)
                throw new ValidationException("Id", "El usuario a actualizar debe tener un ID válido");

            ValidateUser(userDto);

            var entity = MapToEntity(userDto);

            try
            {
                await _sqlData.UpdateAsync(entity);
                await _pgData.UpdateAsync(entity);
                await _myData.UpdateAsync(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar usuario con ID: {UserId}", userDto.Id);
                throw;
            }
        }

        public async Task PatchUserAsync(UserDto userDto)
        {
            if (userDto == null || userDto.Id <= 0)
                throw new ValidationException("Id", "El usuario a actualizar debe tener un ID válido");

            var existing = await _sqlData.GetByIdAsync(userDto.Id);
            if (existing == null)
                throw new EntityNotFoundException("Usuario", userDto.Id);

            if (userDto.PersonId != 0) existing.PersonId = userDto.PersonId;
            if (!string.IsNullOrEmpty(userDto.UserName)) existing.UserName = userDto.UserName;
            if (!string.IsNullOrEmpty(userDto.PasswordHash)) existing.PasswordHash = userDto.PasswordHash;
            if (!string.IsNullOrEmpty(userDto.Code)) existing.Code = userDto.Code;
            if (userDto.Active != null) existing.Active = userDto.Active;

            try
            {
                await _sqlData.UpdateAsync(existing);
                await _pgData.UpdateAsync(existing);
                await _myData.UpdateAsync(existing);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar parcialmente el usuario con ID: {UserId}", userDto.Id);
                throw;
            }
        }

        public async Task DisableUserAsync(int id)
        {
            if (id <= 0)
                throw new ValidationException("id", "El ID del usuario debe ser mayor que cero");

            try
            {
                await _sqlData.DisableAsync(id);
                await _pgData.DisableAsync(id);
                await _myData.DisableAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al desactivar usuario con ID: {UserId}", id);
                throw;
            }
        }

        public async Task DeleteUserAsync(int id)
        {
            if (id <= 0)
                throw new ValidationException("id", "El ID del usuario debe ser mayor que cero");

            try
            {
                await _sqlData.DeleteAsync(id);
                await _pgData.DeleteAsync(id);
                await _myData.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar usuario con ID: {UserId}", id);
                throw;
            }
        }

        private void ValidateUser(UserDto dto)
        {
            if (dto == null)
                throw new ValidationException("userDto", "El objeto usuario no puede ser nulo");

            if (dto.PersonId <= 0)
                throw new ValidationException("PersonId", "El ID de la persona es obligatorio");

            if (string.IsNullOrWhiteSpace(dto.UserName))
                throw new ValidationException("Username", "El nombre de usuario es obligatorio");

            if (string.IsNullOrWhiteSpace(dto.PasswordHash))
                throw new ValidationException("Password", "La contraseña es obligatoria");

            if (string.IsNullOrWhiteSpace(dto.Code))
                throw new ValidationException("Code", "El código de usuario es obligatorio");
        }

        private UserDto MapToDto(User entity) => new()
        {
            Id = entity.Id,
            PersonId = entity.PersonId,
            UserName = entity.UserName,
            PasswordHash = entity.PasswordHash,
            Code = entity.Code,
            Active = entity.Active
        };

        private User MapToEntity(UserDto dto) => new()
        {
            Id = dto.Id,
            PersonId = dto.PersonId,
            UserName = dto.UserName,
            PasswordHash = dto.PasswordHash,
            Code = dto.Code,
            Active = dto.Active
        };
    }
}
