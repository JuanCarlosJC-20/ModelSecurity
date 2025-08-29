using Business.Services;
using Data;
using Entity.Context;
using Entity.DTOs;
using Entity.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Utilities.Exceptions;

namespace Business
{
    public class UserBusiness
    {
        private readonly MultiDatabaseService _multiDbService;
        private readonly ILogger<UserBusiness> _logger;

        public UserBusiness(
            MultiDatabaseService multiDbService,
            ILogger<UserBusiness> logger)
        {
            _multiDbService = multiDbService;
            _logger = logger;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            try
            {
                // Obtener con failover automático
                var users = await _multiDbService.ExecuteWithFailoverAsync(async (context) =>
                {
                    return await context.Set<User>()
                        .Where(u => u.Active)
                        .ToListAsync();
                });

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

            try
            {
                var user = await _multiDbService.ExecuteWithFailoverAsync(async (context) =>
                {
                    return await context.Set<User>()
                        .FirstOrDefaultAsync(u => u.Id == id);
                });

                if (user == null)
                    throw new EntityNotFoundException("Usuario", id);

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
            ValidateUser(userDto);

            var entity = MapToEntity(userDto);
            entity.CreateAt = DateTime.UtcNow;

            try
            {
                // Crear en todas las bases de datos
                await _multiDbService.ExecuteInAllDatabasesAsync(async (context) =>
                {
                    context.Set<User>().Add(entity);
                    await context.SaveChangesAsync();
                });

                return MapToDto(entity);
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
                // Verificar que existe con failover
                var existing = await _multiDbService.ExecuteWithFailoverAsync(async (context) =>
                {
                    return await context.Set<User>()
                        .FirstOrDefaultAsync(u => u.Id == userDto.Id);
                });

                if (existing == null)
                    throw new EntityNotFoundException("Usuario", userDto.Id);

                // Actualizar en todas las bases de datos
                await _multiDbService.ExecuteInAllDatabasesAsync(async (context) =>
                {
                    var userToUpdate = await context.Set<User>()
                        .FirstOrDefaultAsync(u => u.Id == userDto.Id);

                    if (userToUpdate != null)
                    {
                        userToUpdate.PersonId = userDto.PersonId;
                        userToUpdate.UserName = userDto.UserName;
                        userToUpdate.PasswordHash = userDto.PasswordHash;
                        userToUpdate.Code = userDto.Code;
                        userToUpdate.Active = userDto.Active;
                        await context.SaveChangesAsync();
                    }
                });
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
                // Obtener existente con failover
                var existing = await _multiDbService.ExecuteWithFailoverAsync(async (context) =>
                {
                    return await context.Set<User>()
                        .FirstOrDefaultAsync(u => u.Id == userDto.Id);
                });

                if (existing == null)
                    throw new EntityNotFoundException("Usuario", userDto.Id);

                // Actualizar parcialmente en todas las bases de datos
                await _multiDbService.ExecuteInAllDatabasesAsync(async (context) =>
                {
                    var userToUpdate = await context.Set<User>()
                        .FirstOrDefaultAsync(u => u.Id == userDto.Id);

                    if (userToUpdate != null)
                    {
                        if (userDto.PersonId != 0) userToUpdate.PersonId = userDto.PersonId;
                        if (!string.IsNullOrEmpty(userDto.UserName)) userToUpdate.UserName = userDto.UserName;
                        if (!string.IsNullOrEmpty(userDto.PasswordHash)) userToUpdate.PasswordHash = userDto.PasswordHash;
                        if (!string.IsNullOrEmpty(userDto.Code)) userToUpdate.Code = userDto.Code;
                        if (userDto.Active != null) userToUpdate.Active = userDto.Active;
                        await context.SaveChangesAsync();
                    }
                });
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
                // Deshabilitar en todas las bases de datos
                await _multiDbService.ExecuteInAllDatabasesAsync(async (context) =>
                {
                    var userToDisable = await context.Set<User>()
                        .FirstOrDefaultAsync(u => u.Id == id);

                    if (userToDisable != null)
                    {
                        userToDisable.Active = false;
                        userToDisable.DeleteAt = DateTime.UtcNow;
                        await context.SaveChangesAsync();
                    }
                });
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
                // Eliminar de todas las bases de datos
                await _multiDbService.ExecuteInAllDatabasesAsync(async (context) =>
                {
                    var userToDelete = await context.Set<User>()
                        .FirstOrDefaultAsync(u => u.Id == id);

                    if (userToDelete != null)
                    {
                        context.Set<User>().Remove(userToDelete);
                        await context.SaveChangesAsync();
                    }
                });
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
