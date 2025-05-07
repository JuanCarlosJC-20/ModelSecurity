using Data;
using Entity.DTOs;
using Entity.Model;
using Microsoft.Extensions.Logging;
using Utilities.Exceptions;

namespace Business
{
    public class UserBusiness
    {
        private readonly UserData _userData;
        private readonly ILogger<UserBusiness> _logger;

        public UserBusiness(UserData userData, ILogger<UserBusiness> logger)
        {
            _userData = userData;
            _logger = logger;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            try
            {
                var users = await _userData.GetAllAsync();
                return users.Select(u => new UserDto
                {
                    Id = u.Id,
                    PersonId = u.PersonId,
                    UserName = u.UserName,
                    PasswordHash = u.PasswordHash,
                    Code = u.Code,
                    Active = u.Active
                });
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

            try
            {
                var user = await _userData.GetByIdAsync(id);
                if (user == null)
                    throw new EntityNotFoundException("Usuario", id);

                return new UserDto
                {
                    Id = user.Id,
                    PersonId = user.PersonId,
                    UserName = user.UserName,
                    PasswordHash = user.PasswordHash,
                    Code = user.Code,
                    Active = user.Active
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el usuario con ID: {UserId}", id);
                throw new ExternalServiceException("Base de datos", $"Error al recuperar el usuario con ID {id}", ex);
            }
        }

        public async Task<UserDto> CreateUserAsync(UserDto userDto)
        {
            try
            {
                ValidateUser(userDto);
                var user = new User
                {
                    PersonId = userDto.PersonId,
                    UserName = userDto.UserName,
                    PasswordHash = userDto.PasswordHash,
                    Code = userDto.Code,
                    Active = userDto.Active,
                    CreateAt = DateTime.Now
                };

                var createdUser = await _userData.CreateAsync(user);
                return new UserDto
                {
                    Id = createdUser.Id,
                    PersonId = createdUser.PersonId,
                    UserName = createdUser.UserName,
                    PasswordHash = createdUser.PasswordHash,
                    Code = createdUser.Code,
                    Active = createdUser.Active
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario: {UserName}", userDto?.UserName ?? "null");
                throw new ExternalServiceException("Base de datos", "Error al crear el usuario", ex);
            }
        }

        public async Task UpdateUserAsync(UserDto userDto)
        {
            if (userDto == null || userDto.Id <= 0)
                throw new ValidationException("Id", "El usuario a actualizar debe tener un ID válido");

            ValidateUser(userDto);

            try
            {
                var existing = await _userData.GetByIdAsync(userDto.Id);
                if (existing == null)
                    throw new EntityNotFoundException("Usuario", userDto.Id);

                existing.PersonId = userDto.PersonId;
                existing.UserName = userDto.UserName;
                existing.PasswordHash = userDto.PasswordHash;
                existing.Code = userDto.Code;
                existing.Active = userDto.Active;

                var result = await _userData.UpdateAsync(existing);
                if (!result)
                    throw new ExternalServiceException("Base de datos", "Error al actualizar el usuario");
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

            try
            {
                var existing = await _userData.GetByIdAsync(userDto.Id);
                if (existing == null)
                    throw new EntityNotFoundException("Usuario", userDto.Id);

                if (userDto.PersonId != 0)
                    existing.PersonId = userDto.PersonId;

                if (!string.IsNullOrEmpty(userDto.UserName))
                    existing.UserName = userDto.UserName;

                if (!string.IsNullOrEmpty(userDto.PasswordHash))
                    existing.PasswordHash = userDto.PasswordHash;

                if (!string.IsNullOrEmpty(userDto.Code))
                    existing.Code = userDto.Code;

                if (userDto.Active != null)
                    existing.Active = userDto.Active;

                var result = await _userData.UpdateAsync(existing);
                if (!result)
                    throw new ExternalServiceException("Base de datos", "Error al actualizar parcialmente el usuario");
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
                var existing = await _userData.GetByIdAsync(id);
                if (existing == null)
                    throw new EntityNotFoundException("Usuario", id);

                var result = await _userData.DisableAsync(id);
                if (!result)
                    throw new ExternalServiceException("Base de datos", "No se pudo desactivar el usuario");
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
                var existing = await _userData.GetByIdAsync(id);
                if (existing == null)
                    throw new EntityNotFoundException("Usuario", id);

                var result = await _userData.DeleteAsync(id);
                if (!result)
                    throw new ExternalServiceException("Base de datos", "No se pudo eliminar el usuario");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar usuario con ID: {UserId}", id);
                throw;
            }
        }

        private void ValidateUser(UserDto userDto)
        {
            if (userDto == null)
                throw new ValidationException("userDto", "El objeto usuario no puede ser nulo");

            if (userDto.PersonId <= 0)
                throw new ValidationException("PersonId", "El ID de la persona es obligatorio");

            if (string.IsNullOrWhiteSpace(userDto.UserName))
                throw new ValidationException("Username", "El nombre de usuario es obligatorio");

            if (string.IsNullOrWhiteSpace(userDto.PasswordHash))
                throw new ValidationException("Password", "La contraseña es obligatoria");

            if (string.IsNullOrWhiteSpace(userDto.Code))
                throw new ValidationException("Username", "El Code de usuario es obligatorio");
        }
    }
}
