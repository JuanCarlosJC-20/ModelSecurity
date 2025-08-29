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
    public class ModuleBusiness
    {
        private readonly MultiDatabaseService _multiDbService;
        private readonly ILogger<ModuleBusiness> _logger;

        public ModuleBusiness(
            MultiDatabaseService multiDbService,
            ILogger<ModuleBusiness> logger)
        {
            _multiDbService = multiDbService;
            _logger = logger;
        }

        public async Task<IEnumerable<ModuleDto>> GetAllModuleAsync()
        {
            try
            {
                // Obtener con failover automático
                var modules = await _multiDbService.ExecuteWithFailoverAsync(async (context) =>
                {
                    return await context.Set<Module>()
                        .Where(m => m.Active)
                        .ToListAsync();
                });

                return MapToDtoList(modules);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los módulos");
                throw new ExternalServiceException("Base de datos", "Error al recuperar la lista de módulos", ex);
            }
        }

        public async Task<ModuleDto> GetModuleByIdAsync(int id)
        {
            if (id <= 0)
                throw new ValidationException("id", "El ID del módulo debe ser mayor que cero");

            try
            {
                var module = await _multiDbService.ExecuteWithFailoverAsync(async (context) =>
                {
                    return await context.Set<Module>()
                        .FirstOrDefaultAsync(m => m.Id == id);
                });

                if (module == null)
                    throw new EntityNotFoundException("Module", id);

                return MapToDto(module);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el módulo con ID: {ModuleId}", id);
                throw new ExternalServiceException("Base de datos", $"Error al recuperar el módulo con ID {id}", ex);
            }
        }

        public async Task<ModuleDto> CreateModuleAsync(ModuleDto moduleDto)
        {
            try
            {
                ValidateModule(moduleDto);
                var module = MapToEntity(moduleDto);
                module.CreateAt = DateTime.UtcNow;

                // Crear en todas las bases de datos
                await _multiDbService.ExecuteInAllDatabasesAsync(async (context) =>
                {
                    context.Set<Module>().Add(module);
                    await context.SaveChangesAsync();
                });

                return MapToDto(module);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear módulo: {ModuleName}", moduleDto?.Name);
                throw new ExternalServiceException("Base de datos", "Error al crear el módulo", ex);
            }
        }

        public async Task<ModuleDto> UpdateModuleAsync(ModuleDto moduleDto)
        {
            if (moduleDto?.Id <= 0)
                throw new ValidationException("Id", "El ID del módulo debe ser mayor que cero");

            ValidateModule(moduleDto);

            try
            {
                // Verificar que existe con failover
                var existing = await _multiDbService.ExecuteWithFailoverAsync(async (context) =>
                {
                    return await context.Set<Module>()
                        .FirstOrDefaultAsync(m => m.Id == moduleDto.Id);
                });

                if (existing == null)
                    throw new EntityNotFoundException("Module", moduleDto.Id);

                // Actualizar en todas las bases de datos
                await _multiDbService.ExecuteInAllDatabasesAsync(async (context) =>
                {
                    var moduleToUpdate = await context.Set<Module>()
                        .FirstOrDefaultAsync(m => m.Id == moduleDto.Id);

                    if (moduleToUpdate != null)
                    {
                        moduleToUpdate.Name = moduleDto.Name;
                        moduleToUpdate.Active = moduleDto.Active;
                        await context.SaveChangesAsync();
                    }
                });

                return MapToDto(existing);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar módulo con ID: {ModuleId}", moduleDto.Id);
                throw;
            }
        }

        public async Task DeleteModuleAsync(int id)
        {
            if (id <= 0)
                throw new ValidationException("id", "El ID del módulo debe ser mayor que cero");

            try
            {
                // Eliminar de todas las bases de datos
                await _multiDbService.ExecuteInAllDatabasesAsync(async (context) =>
                {
                    var moduleToDelete = await context.Set<Module>()
                        .FirstOrDefaultAsync(m => m.Id == id);

                    if (moduleToDelete != null)
                    {
                        context.Set<Module>().Remove(moduleToDelete);
                        await context.SaveChangesAsync();
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar módulo con ID: {ModuleId}", id);
                throw;
            }
        }

        public async Task DisableModuleAsync(int id)
        {
            if (id <= 0)
                throw new ValidationException("id", "El ID del módulo debe ser mayor que cero");

            try
            {
                // Deshabilitar en todas las bases de datos
                await _multiDbService.ExecuteInAllDatabasesAsync(async (context) =>
                {
                    var moduleToDisable = await context.Set<Module>()
                        .FirstOrDefaultAsync(m => m.Id == id);

                    if (moduleToDisable != null)
                    {
                        moduleToDisable.Active = false;
                        moduleToDisable.DeleteAt = DateTime.UtcNow;
                        await context.SaveChangesAsync();
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al desactivar módulo con ID: {ModuleId}", id);
                throw;
            }
        }

        public async Task PartialUpdateModuleAsync(ModuleDto moduleDto)
        {
            try
            {
                // Obtener existente con failover
                var existingModule = await _multiDbService.ExecuteWithFailoverAsync(async (context) =>
                {
                    return await context.Set<Module>()
                        .FirstOrDefaultAsync(m => m.Id == moduleDto.Id);
                });

                if (existingModule == null)
                    throw new EntityNotFoundException($"No se encontró el módulo con ID {moduleDto.Id}");

                // Actualizar parcialmente en todas las bases de datos
                await _multiDbService.ExecuteInAllDatabasesAsync(async (context) =>
                {
                    var moduleToUpdate = await context.Set<Module>()
                        .FirstOrDefaultAsync(m => m.Id == moduleDto.Id);

                    if (moduleToUpdate != null)
                    {
                        if (!string.IsNullOrEmpty(moduleDto.Name))
                            moduleToUpdate.Name = moduleDto.Name;
                        moduleToUpdate.Active = moduleDto.Active;
                        await context.SaveChangesAsync();
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar parcialmente el módulo con ID: {ModuleId}", moduleDto.Id);
                throw;
            }
        }

        private void ValidateModule(ModuleDto moduleDto)
        {
            if (moduleDto == null)
                throw new ValidationException("El objeto módulo no puede ser nulo");

            if (string.IsNullOrWhiteSpace(moduleDto.Name))
                throw new ValidationException("Name", "El nombre del módulo es obligatorio");
        }

        private ModuleDto MapToDto(Module module) => new()
        {
            Id = module.Id,
            Name = module.Name,
            Active = module.Active
        };

        private Module MapToEntity(ModuleDto dto) => new()
        {
            Id = dto.Id,
            Name = dto.Name,
            Active = dto.Active
        };

        private IEnumerable<ModuleDto> MapToDtoList(IEnumerable<Module> modules)
            => modules.Select(MapToDto);
    }
}