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
    public class RolBusiness
    {
        private readonly MultiDatabaseService _multiDbService;
        private readonly ILogger<RolBusiness> _logger;

        public RolBusiness(
            MultiDatabaseService multiDbService,
            ILogger<RolBusiness> logger)
        {
            _multiDbService = multiDbService;
            _logger = logger;
        }

        public async Task<IEnumerable<RolDto>> GetAllRolesAsync()
        {
            try
            {
                // Obtener con failover automático
                var roles = await _multiDbService.ExecuteWithFailoverAsync(async (context) =>
                {
                    return await context.Set<Rol>()
                        .Where(r => r.Active)
                        .ToListAsync();
                });

                return MapToDtoList(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los roles");
                throw new ExternalServiceException("Base de datos", "Error al recuperar la lista de roles", ex);
            }
        }

        public async Task<RolDto> GetRolByIdAsync(int id)
        {
            if (id <= 0)
                throw new ValidationException("id", "El ID del rol debe ser mayor que cero");

            try
            {
                var rol = await _multiDbService.ExecuteWithFailoverAsync(async (context) =>
                {
                    return await context.Set<Rol>()
                        .FirstOrDefaultAsync(r => r.Id == id);
                });

                if (rol == null)
                    throw new EntityNotFoundException("Rol", id);

                return MapToDto(rol);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el rol con ID: {RolId}", id);
                throw new ExternalServiceException("Base de datos", $"Error al recuperar el rol con ID {id}", ex);
            }
        }

        public async Task<RolDto> CreateRolAsync(RolDto rolDto)
        {
            try
            {
                ValidateRol(rolDto);
                var rol = MapToEntity(rolDto);
                rol.CreateAt = DateTime.UtcNow;

                // Crear en todas las bases de datos
                await _multiDbService.ExecuteInAllDatabasesAsync(async (context) =>
                {
                    context.Set<Rol>().Add(rol);
                    await context.SaveChangesAsync();
                });

                return MapToDto(rol);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear rol: {RolName}", rolDto?.Name);
                throw new ExternalServiceException("Base de datos", "Error al crear el rol", ex);
            }
        }

        public async Task<RolDto> UpdateRolAsync(int id, RolDto rolDto)
        {
            if (id <= 0 || rolDto == null)
                throw new ValidationException("id", "El ID del rol debe ser mayor que cero");

            ValidateRol(rolDto);

            try
            {
                // Verificar que existe con failover
                var existing = await _multiDbService.ExecuteWithFailoverAsync(async (context) =>
                {
                    return await context.Set<Rol>()
                        .FirstOrDefaultAsync(r => r.Id == id);
                });

                if (existing == null)
                    throw new EntityNotFoundException("Rol", id);

                // Actualizar en todas las bases de datos
                await _multiDbService.ExecuteInAllDatabasesAsync(async (context) =>
                {
                    var rolToUpdate = await context.Set<Rol>()
                        .FirstOrDefaultAsync(r => r.Id == id);

                    if (rolToUpdate != null)
                    {
                        rolToUpdate.Name = rolDto.Name;
                        rolToUpdate.Description = rolDto.Description;
                        rolToUpdate.Active = rolDto.Active;
                        await context.SaveChangesAsync();
                    }
                });

                return MapToDto(existing);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar rol con ID: {RolId}", id);
                throw;
            }
        }

        public async Task DeleteRolAsync(int id)
        {
            if (id <= 0)
                throw new ValidationException("id", "El ID del rol debe ser mayor que cero");

            try
            {
                // Eliminar de todas las bases de datos
                await _multiDbService.ExecuteInAllDatabasesAsync(async (context) =>
                {
                    var rolToDelete = await context.Set<Rol>()
                        .FirstOrDefaultAsync(r => r.Id == id);

                    if (rolToDelete != null)
                    {
                        context.Set<Rol>().Remove(rolToDelete);
                        await context.SaveChangesAsync();
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar rol con ID: {RolId}", id);
                throw;
            }
        }

        public async Task DisableRolAsync(int id)
        {
            if (id <= 0)
                throw new ValidationException("id", "El ID del rol debe ser mayor que cero");

            try
            {
                // Deshabilitar en todas las bases de datos
                await _multiDbService.ExecuteInAllDatabasesAsync(async (context) =>
                {
                    var rolToDisable = await context.Set<Rol>()
                        .FirstOrDefaultAsync(r => r.Id == id);

                    if (rolToDisable != null)
                    {
                        rolToDisable.Active = false;
                        rolToDisable.DeleteAt = DateTime.UtcNow;
                        await context.SaveChangesAsync();
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al desactivar rol con ID: {RolId}", id);
                throw;
            }
        }

        public async Task PartialUpdateRolAsync(RolDto rolDto)
        {
            try
            {
                // Obtener existente con failover
                var existingRol = await _multiDbService.ExecuteWithFailoverAsync(async (context) =>
                {
                    return await context.Set<Rol>()
                        .FirstOrDefaultAsync(r => r.Id == rolDto.Id);
                });

                if (existingRol == null)
                    throw new EntityNotFoundException("Rol", rolDto.Id);

                // Actualizar parcialmente en todas las bases de datos
                await _multiDbService.ExecuteInAllDatabasesAsync(async (context) =>
                {
                    var rolToUpdate = await context.Set<Rol>()
                        .FirstOrDefaultAsync(r => r.Id == rolDto.Id);

                    if (rolToUpdate != null)
                    {
                        if (!string.IsNullOrEmpty(rolDto.Name))
                            rolToUpdate.Name = rolDto.Name;
                        if (!string.IsNullOrEmpty(rolDto.Description))
                            rolToUpdate.Description = rolDto.Description;
                        rolToUpdate.Active = rolDto.Active;
                        await context.SaveChangesAsync();
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar parcialmente el rol con ID: {RolId}", rolDto.Id);
                throw;
            }
        }

        private void ValidateRol(RolDto rolDto)
        {
            if (rolDto == null)
                throw new ValidationException("El objeto rol no puede ser nulo");

            if (string.IsNullOrWhiteSpace(rolDto.Name))
                throw new ValidationException("Name", "El nombre del rol es obligatorio");
        }

        private RolDto MapToDto(Rol rol) => new()
        {
            Id = rol.Id,
            Name = rol.Name,
            Description = rol.Description,
            Active = rol.Active
        };

        private Rol MapToEntity(RolDto dto) => new()
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            Active = dto.Active
        };

        private IEnumerable<RolDto> MapToDtoList(IEnumerable<Rol> roles)
            => roles.Select(MapToDto);
    }
}
