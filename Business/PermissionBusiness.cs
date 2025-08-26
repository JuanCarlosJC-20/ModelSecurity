using Data;
using Entity.Context;
using Entity.DTOs;
using Entity.Model;
using Microsoft.Extensions.Logging;
using Utilities.Exceptions;

namespace Business
{
    public class PermissionBusiness
    {
        private readonly PermissionData<SqlServerDbContext> _sqlPermissionData;
        private readonly PermissionData<PostgresDbContext> _pgPermissionData;
        private readonly PermissionData<MySqlDbContext> _myPermissionData;
        private readonly ILogger<PermissionBusiness> _logger;

        public PermissionBusiness(
            PermissionData<SqlServerDbContext> sqlPermissionData,
            PermissionData<PostgresDbContext> pgPermissionData,
            PermissionData<MySqlDbContext> myPermissionData,
            ILogger<PermissionBusiness> logger)
        {
            _sqlPermissionData = sqlPermissionData;
            _pgPermissionData = pgPermissionData;
            _myPermissionData = myPermissionData;
            _logger = logger;
        }

        public async Task<IEnumerable<PermissionDto>> GetAllPermissionAsync()
        {
            try
            {
                var permissions = await _sqlPermissionData.GetAllAsync();
                return MapToDtoList(permissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los permisos");
                throw new ExternalServiceException("Base de datos", "Error al recuperar la lista de permisos", ex);
            }
        }

        public async Task<PermissionDto> GetPermissionByIdAsync(int id)
        {
            if (id <= 0)
                throw new ValidationException("id", "El ID del permiso debe ser mayor que cero");

            try
            {
                var permission = await _sqlPermissionData.GetByIdAsync(id);
                if (permission == null)
                    throw new EntityNotFoundException("Permission", id);

                return MapToDto(permission);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el permiso con ID: {PermissionId}", id);
                throw;
            }
        }

        public async Task<PermissionDto> CreatePermissionAsync(PermissionDto permissionDto)
        {
            try
            {
                ValidatePermission(permissionDto);
                var permission = MapToEntity(permissionDto);
                permission.CreateAt =DateTime.UtcNow;

                // Crear en todas las bases de datos
                var sqlPermission = await _sqlPermissionData.CreateAsync(permission);
                await _pgPermissionData.CreateAsync(permission);
                await _myPermissionData.CreateAsync(permission);

                return MapToDto(sqlPermission);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear permiso: {PermissionName}", permissionDto?.Name);
                throw;
            }
        }

        public async Task<PermissionDto> UpdatePermissionAsync(PermissionDto permissionDto)
        {
            if (permissionDto?.Id <= 0)
                throw new ValidationException("Id", "El ID del permiso debe ser mayor que cero");

            ValidatePermission(permissionDto);

            try
            {
                var permission = MapToEntity(permissionDto);

                // Actualizar en todas las bases de datos
                var updateResult = await _sqlPermissionData.UpdateAsync(permission);
                await _pgPermissionData.UpdateAsync(permission);
                await _myPermissionData.UpdateAsync(permission);

                if (!updateResult)
                    throw new EntityNotFoundException("Permission", permission.Id);

                var updatedPermission = await _sqlPermissionData.GetByIdAsync(permission.Id);
                return MapToDto(updatedPermission);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar permiso con ID: {PermissionId}", permissionDto.Id);
                throw;
            }
        }

        public async Task DeletePermissionAsync(int id)
        {
            if (id <= 0)
                throw new ValidationException("id", "El ID del permiso debe ser mayor que cero");

            try
            {
                // Eliminar de todas las bases de datos
                await _sqlPermissionData.DeleteAsync(id);
                await _pgPermissionData.DeleteAsync(id);
                await _myPermissionData.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar permiso con ID: {PermissionId}", id);
                throw;
            }
        }

        public async Task DisablePermissionAsync(int id)
        {
            if (id <= 0)
                throw new ValidationException("id", "El ID del permiso debe ser mayor que cero");

            try
            {
                // Deshabilitar en todas las bases de datos
                await _sqlPermissionData.DisableAsync(id);
                await _pgPermissionData.DisableAsync(id);
                await _myPermissionData.DisableAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al desactivar permiso con ID: {PermissionId}", id);
                throw;
            }
        }

        public async Task PartialUpdatePermissionAsync(PermissionDto permissionDto)
        {
            try
            {
                var existingPermission = await _sqlPermissionData.GetByIdAsync(permissionDto.Id);
                if (existingPermission == null)
                    throw new EntityNotFoundException($"No se encontró el permiso con ID {permissionDto.Id}");

                if (!string.IsNullOrEmpty(permissionDto.Name))
                    existingPermission.Name = permissionDto.Name;
                if (!string.IsNullOrEmpty(permissionDto.Code))
                    existingPermission.Code = permissionDto.Code;
                existingPermission.Active = permissionDto.Active;

                // Actualizar parcialmente en todas las bases de datos
                await _sqlPermissionData.PartialUpdatePermissionAsync(existingPermission,
                    nameof(existingPermission.Name),
                    nameof(existingPermission.Code),
                    nameof(existingPermission.Active));
                await _pgPermissionData.PartialUpdatePermissionAsync(existingPermission,
                    nameof(existingPermission.Name),
                    nameof(existingPermission.Code),
                    nameof(existingPermission.Active));
                await _myPermissionData.PartialUpdatePermissionAsync(existingPermission,
                    nameof(existingPermission.Name),
                    nameof(existingPermission.Code),
                    nameof(existingPermission.Active));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar parcialmente el permiso con ID: {PermissionId}", permissionDto.Id);
                throw;
            }
        }

        private void ValidatePermission(PermissionDto permissionDto)
        {
            if (permissionDto == null)
                throw new ValidationException("El objeto permiso no puede ser nulo");

            if (string.IsNullOrWhiteSpace(permissionDto.Name))
                throw new ValidationException("Name", "El nombre del permiso es obligatorio");

            if (string.IsNullOrWhiteSpace(permissionDto.Code))
                throw new ValidationException("Code", "El código del permiso es obligatorio");
        }

        private PermissionDto MapToDto(Permission permission) => new()
        {
            Id = permission.Id,
            Name = permission.Name,
            Code = permission.Code,
            Active = permission.Active
        };

        private Permission MapToEntity(PermissionDto dto) => new()
        {
            Id = dto.Id,
            Name = dto.Name,
            Code = dto.Code,
            Active = dto.Active
        };

        private IEnumerable<PermissionDto> MapToDtoList(IEnumerable<Permission> permissions)
            => permissions.Select(MapToDto);
    }
}