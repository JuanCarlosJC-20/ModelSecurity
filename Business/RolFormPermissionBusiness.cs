using Data;
using Entity.DTOs;
using Entity.Model;
using Microsoft.Extensions.Logging;
using Utilities.Exceptions;

namespace Business
{
    /// <summary>
    /// Clase de negocio encargada de la lógica relacionada con los RolFormPermission del sistema.
    /// </summary>
    public class RolFormPermissionBusiness
    {
        private readonly RolFormPermissionData _rolFormPermissionData;
        private readonly ILogger<RolFormPermissionBusiness> _logger;

        public RolFormPermissionBusiness(RolFormPermissionData rolFormPermissionData, ILogger<RolFormPermissionBusiness> logger)
        {
            _rolFormPermissionData = rolFormPermissionData;
            _logger = logger;
        }

        public async Task<IEnumerable<RolFormPermissionDto>> GetAllRolFormPermissionsAsync()
        {
            try
            {
                var rolFormPermissions = await _rolFormPermissionData.GetAllAsync();
                return MapToDtoList(rolFormPermissions);
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
            {
                _logger.LogWarning("ID inválido: {RolFormPermissionId}", id);
                throw new ValidationException("id", "El ID del rolFormPermission debe ser mayor que cero");
            }

            try
            {
                var entity = await _rolFormPermissionData.GetByIdAsync(id);
                if (entity == null)
                {
                    _logger.LogInformation("No se encontró ningún rolFormPermission con ID: {RolFormPermissionId}", id);
                    throw new EntityNotFoundException("RolFormPermission", id);
                }

                return MapToDto(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el rolFormPermission con ID: {RolFormPermissionId}", id);
                throw new ExternalServiceException("Base de datos", $"Error al recuperar el rolFormPermission con ID {id}", ex);
            }
        }

        public async Task<RolFormPermissionDto> CreateRolFormPermissionAsync(RolFormPermissionDto dto)
        {
            try
            {
                ValidateRolFormPermission(dto);
                var entity = MapToEntity(dto);
                var created = await _rolFormPermissionData.CreateAsync(entity);
                return MapToDto(created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear nuevo rolFormPermission con RolId: {RolId}, PermissionId: {PermissionId}, FormId: {FormId}", dto?.RolId, dto?.PermissionId, dto?.FormId);
                throw new ExternalServiceException("Base de datos", "Error al crear el rolFormPermission", ex);
            }
        }

        public async Task<RolFormPermissionDto> UpdateRolFormPermissionAsync(int id, RolFormPermissionDto dto)
        {
            if (dto == null || dto.Id <= 0)
                throw new ValidationException("id", "El ID del rolFormPermission debe ser mayor que cero");

            try
            {
                ValidateRolFormPermission(dto);

                var existing = await _rolFormPermissionData.GetByIdAsync(dto.Id);
                if (existing == null)
                {
                    _logger.LogInformation("RolFormPermission no encontrado para actualizar: {Id}", dto.Id);
                    throw new EntityNotFoundException("RolFormPermission", dto.Id);
                }

                var updated = await _rolFormPermissionData.UpdateAsync(MapToEntity(dto));
                return MapToDto(updated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar rolFormPermission con ID: {Id}", dto.Id);
                throw new ExternalServiceException("Base de datos", "Error al actualizar el rolFormPermission", ex);
            }
        }

        private RolFormPermissionDto MapToDto(bool updated)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteRolFormPermissionAsync(int id)
        {
            if (id <= 0)
                throw new ValidationException("id", "El ID del rolFormPermission debe ser mayor que cero");

            try
            {
                var existing = await _rolFormPermissionData.GetByIdAsync(id);
                if (existing == null)
                {
                    _logger.LogInformation("RolFormPermission no encontrado para eliminar con ID: {Id}", id);
                    throw new EntityNotFoundException("RolFormPermission", id);
                }

                await _rolFormPermissionData.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar rolFormPermission con ID: {Id}", id);
                throw new ExternalServiceException("Base de datos", "Error al eliminar el rolFormPermission", ex);
            }
        }

        private void ValidateRolFormPermission(RolFormPermissionDto dto)
        {
            if (dto == null)
                throw new ValidationException("El objeto rolFormPermission no puede ser nulo");

            if (dto.RolId <= 0)
            {
                _logger.LogWarning("RolId inválido al crear/actualizar rolFormPermission");
                throw new ValidationException("RolId", "El RolId del rolFormPermission es obligatorio y debe ser mayor que cero");
            }

            if (dto.PermissionId <= 0)
            {
                _logger.LogWarning("PermissionId inválido al crear/actualizar rolFormPermission");
                throw new ValidationException("PermissionId", "El PermissionId del rolFormPermission es obligatorio y debe ser mayor que cero");
            }

            if (dto.FormId <= 0)
            {
                _logger.LogWarning("FormId inválido al crear/actualizar rolFormPermission");
                throw new ValidationException("FormId", "El FormId del rolFormPermission es obligatorio y debe ser mayor que cero");
            }
        }

        private RolFormPermissionDto MapToDto(RolFormPermission entity)
        {
            return new RolFormPermissionDto
            {
                Id = entity.Id,
                RolId = entity.RolId,
                PermissionId = entity.PermissionId,
                FormId = entity.FormId
            };
        }

        private RolFormPermission MapToEntity(RolFormPermissionDto dto)
        {
            return new RolFormPermission
            {
                Id = dto.Id,
                RolId = dto.RolId,
                PermissionId = dto.PermissionId,
                FormId = dto.FormId
            };
        }

        private IEnumerable<RolFormPermissionDto> MapToDtoList(IEnumerable<RolFormPermission> entities)
        {
            return entities.Select(MapToDto).ToList();
        }
    }
}
