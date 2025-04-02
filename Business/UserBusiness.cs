using System.ComponentModel.DataAnnotations;
using Data;
using Entity.Dto;
using Entity.Model;
using Microsoft.Extensions.Logging;
using Utilities.Exceptions;

// Definir alias para evitar la referencia ambigua
using CustomValidationException = Utilities.Exceptions.ValidationException;
using DataAnnotationsValidationException = System.ComponentModel.DataAnnotations.ValidationException;

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
                return users.Select(MapToDto).ToList();
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
            {
                _logger.LogWarning("Se intentó obtener un usuario con ID inválido: {UserId}", id);
                throw new CustomValidationException("id", "El ID del usuario debe ser mayor que cero");
            }

            try
            {
                var user = await _userData.GetByIdAsync(id);
                if (user == null)
                {
                    _logger.LogInformation("No se encontró ningún usuario con ID: {UserId}", id);
                    throw new EntityNotFoundException("User", id);
                }

                return MapToDto(user);
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
                    UserName = userDto.UserName,
                    Password = userDto.Password,
                    Email = userDto.Email,
                    RegistrationDate = DateTime.UtcNow,
                    NotificationsEnabled = userDto.NotificationsEnabled
                };

                var createdUser = await _userData.CreateAsync(user);
                return MapToDto(createdUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear un nuevo usuario");
                throw new ExternalServiceException("Base de datos", "Error al crear el usuario", ex);
            }
        }

        public async Task<UserDto> UpdateUserAsync(UserDto userDto)
        {
            try
            {
                ValidateUser(userDto);

                var user = new User
                {
                    Id = userDto.Id,
                    UserName = userDto.UserName,
                    Password = userDto.Password,
                    Email = userDto.Email,
                    RegistrationDate = userDto.RegistrationDate,
                    NotificationsEnabled = userDto.NotificationsEnabled
                };

                var updated = await _userData.UpdateAsync(user);
                if (!updated)
                {
                    throw new ExternalServiceException("Base de datos", "Error al actualizar el usuario");
                }

                return userDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el usuario: {UserName}", userDto?.UserName ?? "null");
                throw new ExternalServiceException("Base de datos", "Error al actualizar el usuario", ex);
            }
        }

        public async Task DeleteUserAsync(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Se intentó eliminar un usuario con ID inválido: {UserId}", id);
                throw new CustomValidationException("id", "El ID del usuario debe ser mayor que cero");
            }

            try
            {
                var deleted = await _userData.DeleteAsync(id);
                if (!deleted)
                {
                    throw new ExternalServiceException("Base de datos", "Error al eliminar el usuario");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el usuario con ID: {UserId}", id);
                throw new ExternalServiceException("Base de datos", $"Error al eliminar el usuario con ID {id}", ex);
            }
        }

        private void ValidateUser(UserDto userDto)
        {
            if (userDto == null)
            {
                throw new CustomValidationException("El objeto usuario no puede ser nulo");
            }

            if (string.IsNullOrWhiteSpace(userDto.UserName))
            {
                _logger.LogWarning("Se intentó crear/actualizar un usuario con UserName vacío");
                throw new CustomValidationException("UserName", "El nombre de usuario es obligatorio");
            }

            if (string.IsNullOrWhiteSpace(userDto.Email))
            {
                _logger.LogWarning("Se intentó crear/actualizar un usuario con Email vacío");
                throw new DataAnnotationsValidationException("El email es obligatorio");
            }

            if (string.IsNullOrWhiteSpace(userDto.Password))
            {
                _logger.LogWarning("Se intentó crear/actualizar un usuario con Password vacío");
                throw new CustomValidationException("Password", "La contraseña es obligatoria");
            }
        }

        private static UserDto MapToDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                RegistrationDate = user.RegistrationDate,
                NotificationsEnabled = user.NotificationsEnabled
            };
        }

        private static User MapToEntity(UserDto userDto)
        {
            return new User
            {
                Id = userDto.Id,
                UserName = userDto.UserName,
                Email = userDto.Email,
                RegistrationDate = userDto.RegistrationDate,
                NotificationsEnabled = userDto.NotificationsEnabled
            };
        }
    }
}

