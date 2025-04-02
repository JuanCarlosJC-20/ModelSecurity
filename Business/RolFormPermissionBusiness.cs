using System.ComponentModel.DataAnnotations;
using Data;
using Entity.Dto;
using Entity.Model;
using Microsoft.Extensions.Logging;
using Utilities.Exceptions;

namespace Business
{
    public class RolFormPermissionBusiness
    {
        private readonly RolFormPermissionData _rolFormPermissionData;
        private readonly ILogger<RolFormPermissionBusiness> _logger;

        public RolFormPermissionBusiness(RolFormPermissionData rolFormPermissionData, ILogger<RolFormPermissionBusiness> logger)
        {
            _rolFormPermissionData = rolFormPermissionData;
            _logger = logger;
        }

        public async Task<IEnumerable<RolFormPermissionDto>> GetAllAsync()
        {
            try
            {
                var permissions = await _rolFormPermissionData.GetAllAsync();
                return permissions.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todas las relaciones Rol-Form-Permiso");
                throw new ExternalServiceException("Base de datos", "Error al recuperar los permisos de los roles", ex);
            }
        }

        public async Task<RolFormPermissionDto> GetByIdAsync(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Se intentó obtener una relación Rol-Form-Permiso con ID inválido: {Id}", id);
                throw new ArgumentException("El ID debe ser mayor que cero", nameof(id));
            }

            try
            {
                var permission = await _rolFormPermissionData.GetByIdAsync(id);
                if (permission == null)
                {
                    _logger.LogInformation("No se encontró ninguna relación Rol-Form-Permiso con ID: {Id}", id);
                    throw new EntityNotFoundException("RolFormPermission", id);
                }

                return MapToDto(permission);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la relación Rol-Form-Permiso con ID: {Id}", id);
                throw new ExternalServiceException("Base de datos", $"Error al recuperar la relación con ID {id}", ex);
            }
        }

        public async Task<RolFormPermissionDto> CreateAsync(RolFormPermissionDto dto)
        {
            try
            {
                Validate(dto);

                var entity = MapToEntity(dto);
                var created = await _rolFormPermissionData.CreateAsync(entity);

                return MapToDto(created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear una nueva relación Rol-Form-Permiso");
                throw new ExternalServiceException("Base de datos", "Error al crear la relación", ex);
            }
        }

        public async Task<RolFormPermissionDto> UpdateAsync(RolFormPermissionDto dto)
        {
            try
            {
                Validate(dto);

                var entity = MapToEntity(dto);
                var updated = await _rolFormPermissionData.UpdateAsync(entity);
                if (!updated)
                {
                    throw new ExternalServiceException("Base de datos", "Error al actualizar la relación");
                }

                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar la relación Rol-Form-Permiso");
                throw new ExternalServiceException("Base de datos", "Error al actualizar la relación", ex);
            }
        }

        public async Task DeleteAsync(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Se intentó eliminar una relación Rol-Form-Permiso con ID inválido: {Id}", id);
                throw new ArgumentException("El ID debe ser mayor que cero", nameof(id));
            }

            try
            {
                var deleted = await _rolFormPermissionData.DeleteAsync(id);
                if (!deleted)
                {
                    throw new ExternalServiceException("Base de datos", "Error al eliminar la relación");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar la relación Rol-Form-Permiso con ID: {Id}", id);
                throw new ExternalServiceException("Base de datos", $"Error al eliminar la relación con ID {id}", ex);
            }
        }

        private void Validate(RolFormPermissionDto dto)
        {
            if (dto == null)
            {
                throw new ArgumentException("El objeto no puede ser nulo", nameof(dto));
            }

            if (dto.RolId <= 0)
            {
                _logger.LogWarning("Se intentó crear/actualizar una relación con RolId inválido");
                throw new ArgumentException("El RolId debe ser mayor que cero", nameof(dto.RolId));
            }

            if (dto.FormId <= 0)
            {
                _logger.LogWarning("Se intentó crear/actualizar una relación con FormId inválido");
                throw new ArgumentException("El FormId debe ser mayor que cero", nameof(dto.FormId));
            }

            if (dto.PermissionId <= 0)
            {
                _logger.LogWarning("Se intentó crear/actualizar una relación con PermissionId inválido");
                throw new ArgumentException("El PermissionId debe ser mayor que cero", nameof(dto.PermissionId));
            }
        }

        private static RolFormPermissionDto MapToDto(RolFormPermission entity)
        {
            return new RolFormPermissionDto
            {
                Id = entity.Id,
                RolId = entity.RolId,
                FormId = entity.FormId,
                PermissionId = entity.PermissionId
            };
        }

        private static RolFormPermission MapToEntity(RolFormPermissionDto dto)
        {
            return new RolFormPermission
            {
                Id = dto.Id,
                RolId = dto.RolId,
                FormId = dto.FormId,
                PermissionId = dto.PermissionId
            };
        }
    }
}