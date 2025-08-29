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
    public class PermissionBusiness
    {
        private readonly MultiDatabaseService _multiDbService;
        private readonly ILogger<PermissionBusiness> _logger;

        public PermissionBusiness(
            MultiDatabaseService multiDbService,
            ILogger<PermissionBusiness> logger)
        {
            _multiDbService = multiDbService;
            _logger = logger;
        }

        public async Task<IEnumerable<PermissionDto>> GetAllPermissionAsync()
        {
            try
            {
                // Obtener con failover automático
                var permissions = await _multiDbService.ExecuteWithFailoverAsync(async (context) =>
                {
                    return await context.Set<Permission>()
                        .Where(p => p.Active)
                        .ToListAsync();
                });

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
                var permission = await _multiDbService.ExecuteWithFailoverAsync(async (context) =>
                {
                    return await context.Set<Permission>()
                        .FirstOrDefaultAsync(p => p.Id == id);
                });

                if (permission == null)
                    throw new EntityNotFoundException("Permission", id);

                return MapToDto(permission);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el permiso con ID: {PermissionId}", id);
                throw new ExternalServiceException("Base de datos", $"Error al recuperar el permiso con ID {id}", ex);
            }
        }

        public async Task<PermissionDto> CreatePermissionAsync(PermissionDto permissionDto)
        {
            try
            {
                ValidatePermission(permissionDto);
                var permission = MapToEntity(permissionDto);
                permission.CreateAt = DateTime.UtcNow;

                // Crear en todas las bases de datos
                await _multiDbService.ExecuteInAllDatabasesAsync(async (context) =>
                {
                    context.Set<Permission>().Add(permission);
                    await context.SaveChangesAsync();
                });

                return MapToDto(permission);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear permiso: {PermissionName}", permissionDto?.Name);
                throw new ExternalServiceException("Base de datos", "Error al crear el permiso", ex);
            }
        }

        public async Task<PermissionDto> UpdatePermissionAsync(PermissionDto permissionDto)
        {
            if (permissionDto?.Id <= 0)
                throw new ValidationException("Id", "El ID del permiso debe ser mayor que cero");

            ValidatePermission(permissionDto);

            try
            {
                // Verificar que existe con failover
                var existing = await _multiDbService.ExecuteWithFailoverAsync(async (context) =>
                {
                    return await context.Set<Permission>()
                        .FirstOrDefaultAsync(p => p.Id == permissionDto.Id);
                });

                if (existing == null)
                    throw new EntityNotFoundException("Permission", permissionDto.Id);

                // Actualizar en todas las bases de datos
                await _multiDbService.ExecuteInAllDatabasesAsync(async (context) =>
                {
                    var permissionToUpdate = await context.Set<Permission>()
                        .FirstOrDefaultAsync(p => p.Id == permissionDto.Id);

                    if (permissionToUpdate != null)
                    {
                        permissionToUpdate.Name = permissionDto.Name;
                        permissionToUpdate.Code = permissionDto.Code;
                        permissionToUpdate.Active = permissionDto.Active;
                        await context.SaveChangesAsync();
                    }
                });

                return MapToDto(existing);
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
                await _multiDbService.ExecuteInAllDatabasesAsync(async (context) =>
                {
                    var permissionToDelete = await context.Set<Permission>()
                        .FirstOrDefaultAsync(p => p.Id == id);

                    if (permissionToDelete != null)
                    {
                        context.Set<Permission>().Remove(permissionToDelete);
                        await context.SaveChangesAsync();
                    }
                });
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
                await _multiDbService.ExecuteInAllDatabasesAsync(async (context) =>
                {
                    var permissionToDisable = await context.Set<Permission>()
                        .FirstOrDefaultAsync(p => p.Id == id);

                    if (permissionToDisable != null)
                    {
                        permissionToDisable.Active = false;
                        permissionToDisable.DeleteAt = DateTime.UtcNow;
                        await context.SaveChangesAsync();
                    }
                });
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
                // Obtener existente con failover
                var existingPermission = await _multiDbService.ExecuteWithFailoverAsync(async (context) =>
                {
                    return await context.Set<Permission>()
                        .FirstOrDefaultAsync(p => p.Id == permissionDto.Id);
                });

                if (existingPermission == null)
                    throw new EntityNotFoundException($"No se encontró el permiso con ID {permissionDto.Id}");

                // Actualizar parcialmente en todas las bases de datos
                await _multiDbService.ExecuteInAllDatabasesAsync(async (context) =>
                {
                    var permissionToUpdate = await context.Set<Permission>()
                        .FirstOrDefaultAsync(p => p.Id == permissionDto.Id);

                    if (permissionToUpdate != null)
                    {
                        if (!string.IsNullOrEmpty(permissionDto.Name))
                            permissionToUpdate.Name = permissionDto.Name;
                        if (!string.IsNullOrEmpty(permissionDto.Code))
                            permissionToUpdate.Code = permissionDto.Code;
                        permissionToUpdate.Active = permissionDto.Active;
                        await context.SaveChangesAsync();
                    }
                });
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