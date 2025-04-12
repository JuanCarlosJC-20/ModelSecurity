using Data;
using Entity.DTOs;
using Entity.Model;
using Microsoft.Extensions.Logging;
using Utilities.Exceptions;

namespace Business
{
    /// <summary>
    /// Clase de negocio encargada de la lógica relacionada con los permisos del sistema.
    /// </summary>
    public class PermissionBusiness
    {
        private readonly PermissionData _permissionData;
        private readonly ILogger<PermissionBusiness> _logger;

        public PermissionBusiness(PermissionData permissionData, ILogger<PermissionBusiness> logger)
        {
            _permissionData = permissionData;
            _logger = logger;
        }

        // Obtener todos los permisos
        public async Task<IEnumerable<PermissionDto>> GetAllPermissionAsync()
        {
            try
            {
                var permissions = await _permissionData.GetAllAsync();
                return MapToDtoList(permissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los permisos");
                throw new ExternalServiceException("Base de datos", "Error al recuperar la lista de permisos", ex);
            }
        }

        // Obtener un permiso por ID
        public async Task<PermissionDto> GetPermissionByIdAsync(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("ID de permiso inválido: {PermissionId}", id);
                throw new ValidationException("id", "El ID del permiso debe ser mayor que cero");
            }

            try
            {
                var permission = await _permissionData.GetByIdAsync(id);
                if (permission == null)
                {
                    _logger.LogInformation("Permiso no encontrado con ID: {PermissionId}", id);
                    throw new EntityNotFoundException("Permission", id);
                }

                return MapToDto(permission);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el permiso con ID: {PermissionId}", id);
                throw new ExternalServiceException("Base de datos", $"Error al recuperar el permiso con ID {id}", ex);
            }
        }

        // Crear un nuevo permiso
        public async Task<PermissionDto> CreatePermissionAsync(PermissionDto permissionDto)
        {
            try
            {
                ValidatePermission(permissionDto);

                var permission = MapToEntity(permissionDto);
                 permission.CreateAt=DateTime.Now;
                var created = await _permissionData.CreateAsync(permission);

                return MapToDto(created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear permiso: {PermissionNombre}", permissionDto?.Name ?? "null");
                throw new ExternalServiceException("Base de datos", "Error al crear el permiso", ex);
            }
        }

        // Actualizar permiso existente
        public async Task<PermissionDto> UpdatePermissionAsync(int id, PermissionDto permissionDto)
        {
            try
            {
                if (permissionDto == null || permissionDto.Id <= 0)
                {
                    throw new ValidationException("Id", "El ID del permiso debe ser mayor que cero");
                }

                ValidatePermission(permissionDto);

                var existing = await _permissionData.GetByIdAsync(permissionDto.Id);
                if (existing == null)
                {
                    _logger.LogInformation("Permiso no encontrado para actualizar: {PermissionId}", permissionDto.Id);
                    throw new EntityNotFoundException("Permission", permissionDto.Id);
                }

                var updated = await _permissionData.UpdateAsync(MapToEntity(permissionDto));
                return MapToDto(updated);
            }
            catch (ValidationException) { throw; }
            catch (EntityNotFoundException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar permiso con ID: {PermissionId}", permissionDto?.Id ?? 0);
                throw new ExternalServiceException("Base de datos", "Error al actualizar el permiso", ex);
            }
        }

        private PermissionDto MapToDto(bool updated)
        {
            throw new NotImplementedException();
        }

        // Eliminar permiso por ID
        public async Task DeletePermissionAsync(int id)
        {
            if (id <= 0)
            {
                throw new ValidationException("id", "El ID del permiso debe ser mayor que cero");
            }

            try
            {
                var existing = await _permissionData.GetByIdAsync(id);
                if (existing == null)
                {
                    _logger.LogInformation("Permiso no encontrado para eliminar con ID: {PermissionId}", id);
                    throw new EntityNotFoundException("Permission", id);
                }

                await _permissionData.DeleteAsync(id);
            }
            catch (ValidationException) { throw; }
            catch (EntityNotFoundException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar permiso con ID: {PermissionId}", id);
                throw new ExternalServiceException("Base de datos", "Error al eliminar el permiso", ex);
            }
        }

        // Validar DTO
        private void ValidatePermission(PermissionDto permissionDto)
        {
            if (permissionDto == null)
            {
                throw new ValidationException("El objeto permiso no puede ser nulo");
            }

            if (string.IsNullOrWhiteSpace(permissionDto.Name))
            {
                _logger.LogWarning("Nombre del permiso vacío");
                throw new ValidationException("Name", "El Name del permiso es obligatorio");
            }

            if (string.IsNullOrWhiteSpace(permissionDto.Code))
            {
                _logger.LogWarning("Código del permiso vacío");
                throw new ValidationException("Code", "El Code del permiso es obligatorio");
            }
        }

        // Mapeos
        private PermissionDto MapToDto(Permission permission)
        {
            return new PermissionDto
            {
                Id = permission.Id,
                Name = permission.Name,
                Code = permission.Code,
                Active = permission.Active
            };
        }

        private Permission MapToEntity(PermissionDto dto)
        {
            return new Permission
            {
                Id = dto.Id,
                Name = dto.Name,
                Code = dto.Code,
                Active = dto.Active
            };
        }

        private IEnumerable<PermissionDto> MapToDtoList(IEnumerable<Permission> permissions)
        {
            var list = new List<PermissionDto>();
            foreach (var permission in permissions)
            {
                list.Add(MapToDto(permission));
            }
            return list;
        }
    }
}
