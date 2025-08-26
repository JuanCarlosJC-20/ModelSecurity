using Data;
using Entity.Context;
using Entity.DTOs;
using Entity.Model;
using Microsoft.Extensions.Logging;
using Utilities.Exceptions;

namespace Business
{
    /// <summary>
    /// Clase de negocio encargada de la lógica relacionada con los RolUsers del sistema en múltiples bases de datos.
    /// </summary>
    public class RolUserBusiness
    {
        private readonly RolUserData<SqlServerDbContext> _sqlData;
        private readonly RolUserData<PostgresDbContext> _pgData;
        private readonly RolUserData<MySqlDbContext> _myData;
        private readonly ILogger<RolUserBusiness> _logger;

        public RolUserBusiness(
            RolUserData<SqlServerDbContext> sqlData,
            RolUserData<PostgresDbContext> pgData,
            RolUserData<MySqlDbContext> myData,
            ILogger<RolUserBusiness> logger)
        {
            _sqlData = sqlData;
            _pgData = pgData;
            _myData = myData;
            _logger = logger;
        }

        public async Task<IEnumerable<RolUserDto>> GetAllRolUsersAsync()
        {
            try
            {
                var rolUsers = await _sqlData.GetAllAsync();
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

            var rolUser = await _sqlData.GetByIdAsync(id);
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
                var created = await _sqlData.CreateAsync(entity);
                await _pgData.CreateAsync(entity);
                await _myData.CreateAsync(entity);

                return MapToDto(created);
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

                // Actualizar en todas las bases de datos
                await _sqlData.UpdateAsync(entity);
                await _pgData.UpdateAsync(entity);
                await _myData.UpdateAsync(entity);

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
                await _sqlData.DeleteAsync(id);
                await _pgData.DeleteAsync(id);
                await _myData.DeleteAsync(id);
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
                var all = await _sqlData.GetAllAsync();
                var filtered = all.Where(ru => ru.UserId == userId);
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
                var all = await _sqlData.GetAllAsync();
                var filtered = all.Where(ru => ru.RolId == rolId);
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
