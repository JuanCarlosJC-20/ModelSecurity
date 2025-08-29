using Business.Services;
using Entity.DTOs;
using Entity.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Utilities.Exceptions;

namespace Business
{
    /// <summary>
    /// Clase de negocio encargada de la lógica relacionada con los RolFormPermission del sistema en múltiples bases de datos.
    /// </summary>
    public class RolFormPermissionBusiness
    {
        private readonly MultiDatabaseService _multiDbService;
        private readonly ILogger<RolFormPermissionBusiness> _logger;

        public RolFormPermissionBusiness(
            MultiDatabaseService multiDbService,
            ILogger<RolFormPermissionBusiness> logger)
        {
            _multiDbService = multiDbService;
            _logger = logger;
        }

        public async Task<IEnumerable<RolFormPermissionDto>> GetAllRolFormPermissionsAsync()
        {
            try
            {
                var entities = await _multiDbService.ExecuteWithFailoverAsync(async (context) =>
                {
                    return await context.Set<RolFormPermission>().ToListAsync();
                });
                return MapToDtoList(entities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los rolFormPermissions");
                throw new ExternalServiceException("Base de datos", "Error al recuperar la lista de rolFormPermissions", ex);
            }
        }

        public async Task<RolFormPermissionDto> GetRolFormPermissionByIdAsync(int id)
        {
            if (id <= 0)
                throw new ValidationException("id", "El ID del rolFormPermission debe ser mayor que cero");

            var entity = await _multiDbService.ExecuteWithFailoverAsync(async (context) =>
            {
                return await context.Set<RolFormPermission>().FirstOrDefaultAsync(rfp => rfp.Id == id);
            });

            if (entity == null)
                throw new EntityNotFoundException("RolFormPermission", id);

            return MapToDto(entity);
        }

        public async Task<RolFormPermissionDto> CreateRolFormPermissionAsync(RolFormPermissionDto dto)
        {
            try
            {
                ValidateRolFormPermission(dto);
                var entity = MapToEntity(dto);

                // Crear en todas las bases de datos
                await _multiDbService.ExecuteInAllDatabasesAsync(async (context) =>
                {
                    context.Set<RolFormPermission>().Add(entity);
                    await context.SaveChangesAsync();
                });

                return MapToDto(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error al crear rolFormPermission con RolId: {RolId}, PermissionId: {PermissionId}, FormId: {FormId}",
                    dto?.RolId, dto?.PermissionId, dto?.FormId);
                throw;
            }
        }

        public async Task<RolFormPermissionDto> UpdateRolFormPermissionAsync(int id, RolFormPermissionDto dto)
        {
            if (id <= 0 || dto == null)
                throw new ValidationException("id", "El ID del rolFormPermission debe ser mayor que cero");

            ValidateRolFormPermission(dto);

            try
            {
                var entity = MapToEntity(dto);
                entity.Id = id;

                // Actualizar en todas las bases de datos
                await _multiDbService.ExecuteInAllDatabasesAsync(async (context) =>
                {
                    context.Set<RolFormPermission>().Update(entity);
                    await context.SaveChangesAsync();
                });

                return MapToDto(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar rolFormPermission con ID: {Id}", id);
                throw;
            }
        }

        public async Task DeleteRolFormPermissionAsync(int id)
        {
            if (id <= 0)
                throw new ValidationException("id", "El ID del rolFormPermission debe ser mayor que cero");

            try
            {
                await _multiDbService.ExecuteInAllDatabasesAsync(async (context) =>
                {
                    var entity = await context.Set<RolFormPermission>().FirstOrDefaultAsync(rfp => rfp.Id == id);
                    if (entity != null)
                    {
                        context.Set<RolFormPermission>().Remove(entity);
                        await context.SaveChangesAsync();
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar rolFormPermission con ID: {Id}", id);
                throw;
            }
        }

        private void ValidateRolFormPermission(RolFormPermissionDto dto)
        {
            if (dto == null)
                throw new ValidationException("El objeto rolFormPermission no puede ser nulo");

            if (dto.RolId <= 0)
                throw new ValidationException("RolId", "El RolId debe ser mayor que cero");

            if (dto.PermissionId <= 0)
                throw new ValidationException("PermissionId", "El PermissionId debe ser mayor que cero");

            if (dto.FormId <= 0)
                throw new ValidationException("FormId", "El FormId debe ser mayor que cero");
        }

        private RolFormPermissionDto MapToDto(RolFormPermission entity) => new()
        {
            Id = entity.Id,
            RolId = entity.RolId,
            PermissionId = entity.PermissionId,
            FormId = entity.FormId
        };

        private RolFormPermission MapToEntity(RolFormPermissionDto dto) => new()
        {
            Id = dto.Id,
            RolId = dto.RolId,
            PermissionId = dto.PermissionId,
            FormId = dto.FormId
        };

        private IEnumerable<RolFormPermissionDto> MapToDtoList(IEnumerable<RolFormPermission> entities)
            => entities.Select(MapToDto);
    }
}
