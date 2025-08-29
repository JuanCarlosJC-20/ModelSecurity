using Business.Services;
using Entity.DTOs;
using Entity.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Utilities.Exceptions;

namespace Business
{
    /// <summary>
    /// Clase de negocio encargada de la lógica relacionada con los RolUsers del sistema en múltiples bases de datos.
    /// </summary>
    public class RolUserBusiness
    {
        private readonly MultiDatabaseService _multiDbService;
        private readonly ILogger<RolUserBusiness> _logger;

        public RolUserBusiness(
            MultiDatabaseService multiDbService,
            ILogger<RolUserBusiness> logger)
        {
            _multiDbService = multiDbService;
            _logger = logger;
        }

        public async Task<IEnumerable<RolUserDto>> GetAllRolUsersAsync()
        {
            try
            {
                var rolUsers = await _multiDbService.ExecuteWithFailoverAsync(async (context) =>
                {
                    return await context.Set<RolUser>().ToListAsync();
                });
                return MapToDtoList(rolUsers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los rolUsers");
                throw new ExternalServiceException("Base de datos", "Error al recuperar la lista de rolUsers", ex);
            }
        }

        public async Task<RolUserDto> GetRolUserByIdAsync(int id)
        {
            if (id <= 0)
                throw new ValidationException("id", "El ID del rolUser debe ser mayor que cero");

            var rolUser = await _multiDbService.ExecuteWithFailoverAsync(async (context) =>
            {
                return await context.Set<RolUser>().FirstOrDefaultAsync(ru => ru.Id == id);
            });

            if (rolUser == null)
                throw new EntityNotFoundException("RolUser", id);

            return MapToDto(rolUser);
        }

        public async Task<RolUserDto> CreateRolUserAsync(RolUserDto dto)
        {
            try
            {
                ValidateRolUser(dto);
                var entity = MapToEntity(dto);

                // Crear en todas las bases de datos
                await _multiDbService.ExecuteInAllDatabasesAsync(async (context) =>
                {
                    context.Set<RolUser>().Add(entity);
                    await context.SaveChangesAsync();
                });

                return MapToDto(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear nuevo rolUser con UserId: {UserId}, RolId: {RolId}", dto?.UserId, dto?.RolId);
                throw;
            }
        }

        public async Task<RolUserDto> UpdateRolUserAsync(int id, RolUserDto dto)
        {
            if (id <= 0 || dto == null)
                throw new ValidationException("id", "El ID del rolUser debe ser mayor que cero");

            ValidateRolUser(dto);

            try
            {
                var entity = MapToEntity(dto);
                entity.Id = id;

                // Actualizar en todas las bases de datos
                await _multiDbService.ExecuteInAllDatabasesAsync(async (context) =>
                {
                    context.Set<RolUser>().Update(entity);
                    await context.SaveChangesAsync();
                });

                return MapToDto(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar rolUser con ID: {Id}", id);
                throw;
            }
        }

        public async Task DeleteRolUserAsync(int id)
        {
            if (id <= 0)
                throw new ValidationException("id", "El ID del rolUser debe ser mayor que cero");

            try
            {
                await _multiDbService.ExecuteInAllDatabasesAsync(async (context) =>
                {
                    var entity = await context.Set<RolUser>().FirstOrDefaultAsync(ru => ru.Id == id);
                    if (entity != null)
                    {
                        context.Set<RolUser>().Remove(entity);
                        await context.SaveChangesAsync();
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar rolUser con ID: {Id}", id);
                throw;
            }
        }

        // 🔹 Nuevo: Obtener todos los RolUsers de un usuario
        public async Task<IEnumerable<RolUserDto>> GetRolUsersByUserIdAsync(int userId)
        {
            if (userId <= 0)
                throw new ValidationException("userId", "El userId debe ser mayor que cero.");

            try
            {
                var filtered = await _multiDbService.ExecuteWithFailoverAsync(async (context) =>
                {
                    return await context.Set<RolUser>().Where(ru => ru.UserId == userId).ToListAsync();
                });
                return MapToDtoList(filtered);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener RolUsers por userId {UserId}", userId);
                throw new ExternalServiceException("Base de datos", "No se pudieron leer los RolUsers por usuario.", ex);
            }
        }

        // 🔹 Nuevo: Obtener todos los RolUsers de un rol
        public async Task<IEnumerable<RolUserDto>> GetRolUsersByRolIdAsync(int rolId)
        {
            if (rolId <= 0)
                throw new ValidationException("rolId", "El rolId debe ser mayor que cero.");

            try
            {
                var filtered = await _multiDbService.ExecuteWithFailoverAsync(async (context) =>
                {
                    return await context.Set<RolUser>().Where(ru => ru.RolId == rolId).ToListAsync();
                });
                return MapToDtoList(filtered);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener RolUsers por rolId {RolId}", rolId);
                throw new ExternalServiceException("Base de datos", "No se pudieron leer los RolUsers por rol.", ex);
            }
        }

        private void ValidateRolUser(RolUserDto dto)
        {
            if (dto == null)
                throw new ValidationException("El objeto rolUser no puede ser nulo");

            if (dto.RolId <= 0)
                throw new ValidationException("RolId", "El RolId del rolUser es obligatorio y debe ser mayor que cero");

            if (dto.UserId <= 0)
                throw new ValidationException("UserId", "El UserId del rolUser es obligatorio y debe ser mayor que cero");
        }

        private RolUserDto MapToDto(RolUser entity) => new()
        {
            Id = entity.Id,
            UserId = entity.UserId,
            RolId = entity.RolId
        };

        private RolUser MapToEntity(RolUserDto dto) => new()
        {
            Id = dto.Id,
            UserId = dto.UserId,
            RolId = dto.RolId
        };

        private IEnumerable<RolUserDto> MapToDtoList(IEnumerable<RolUser> entities)
            => entities.Select(MapToDto);
    }
}
