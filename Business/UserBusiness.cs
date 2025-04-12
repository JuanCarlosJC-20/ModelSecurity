using Data;
using Entity.DTOs;
using Entity.Model;
using Microsoft.Extensions.Logging;
using Utilities.Exceptions;

namespace Business
{
    /// <summary>
    /// Clase de negocio encargada de la lógica relacionada con los usuarios del sistema.
    /// </summary>
    public class UserBusiness
    {
        private readonly UserData _userData;
        private readonly PersonData _personData; // 🔍 Inyectamos PersonData
        private readonly ILogger<UserBusiness> _logger;

        public UserBusiness(UserData userData, PersonData personData, ILogger<UserBusiness> logger)
        {
            _userData = userData;
            _personData = personData;
            _logger = logger;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            try
            {
                var users = await _userData.GetAllAsync();
                return MapToDtoList(users);
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
                _logger.LogWarning("ID inválido: {UserId}", id);
                throw new ValidationException("id", "El ID del usuario debe ser mayor que cero");
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

        public async Task<UserDto> CreateUserAsync(UserDto dto)
        {
            try
            {
                ValidateUser(dto);

                //  Validación de PersonId antes de guardar
                var person = await _personData.GetByIdAsync(dto.PersonId);
                if (person == null)
                {
                    _logger.LogWarning("No se encontró la persona con ID: {PersonId}", dto.PersonId);
                    throw new ValidationException("PersonId", "No existe una persona con el ID proporcionado");
                }

                var entity = MapToEntity(dto);
                entity.CreateAt = DateTime.Now;

                var created = await _userData.CreateAsync(entity);

                return MapToDto(created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear nuevo usuario: {UserName}", dto?.UserName ?? "null");
                throw new ExternalServiceException("Base de datos", "Error al crear el usuario", ex);
            }
        }

        public async Task<UserDto> UpdateUserAsync(int id, UserDto dto)
        {
            if (dto == null || dto.Id <= 0)
            {
                throw new ValidationException("id", "El ID del usuario debe ser mayor que cero");
            }

            try
            {
                ValidateUser(dto);

                var existing = await _userData.GetByIdAsync(dto.Id);
                if (existing == null)
                {
                    _logger.LogInformation("Usuario no encontrado para actualizar con ID: {Id}", dto.Id);
                    throw new EntityNotFoundException("User", dto.Id);
                }

                var updated = await _userData.UpdateAsync(MapToEntity(dto));
                return MapToDto(updated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar usuario con ID: {Id}", dto.Id);
                throw new ExternalServiceException("Base de datos", "Error al actualizar el usuario", ex);
            }
        }

        private UserDto MapToDto(bool updated)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteUserAsync(int id)
        {
            if (id <= 0)
                throw new ValidationException("id", "El ID del usuario debe ser mayor que cero");

            try
            {
                var existing = await _userData.GetByIdAsync(id);
                if (existing == null)
                {
                    _logger.LogInformation("Usuario no encontrado para eliminar con ID: {Id}", id);
                    throw new EntityNotFoundException("User", id);
                }

                await _userData.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar usuario con ID: {Id}", id);
                throw new ExternalServiceException("Base de datos", "Error al eliminar el usuario", ex);
            }
        }

        private void ValidateUser(UserDto dto)
        {
            if (dto == null)
                throw new ValidationException("El objeto User no puede ser nulo");

            if (string.IsNullOrWhiteSpace(dto.UserName))
            {
                _logger.LogWarning("UserName vacío al crear/actualizar usuario");
                throw new ValidationException("UserName", "El UserName del usuario es obligatorio");
            }

            if (string.IsNullOrWhiteSpace(dto.Code))
            {
                _logger.LogWarning("Code vacío al crear/actualizar usuario");
                throw new ValidationException("Code", "El código del usuario es obligatorio");
            }
        }

        private UserDto MapToDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Code = user.Code,
                Active = user.Active,
                PersonId = user.PersonId // Agregado para reflejarlo también en el DTO
            };
        }

        private User MapToEntity(UserDto dto)
        {
            return new User
            {
                Id = dto.Id,
                UserName = dto.UserName,
                Code = dto.Code,
                Active = dto.Active,
                PersonId = dto.PersonId // 🔍 Asegura que se asigne correctamente
            };
        }

        private IEnumerable<UserDto> MapToDtoList(IEnumerable<User> users)
        {
            return users.Select(MapToDto).ToList();
        }
    }
}
