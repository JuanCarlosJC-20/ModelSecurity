using Data;
using Entity.Context;
using Entity.DTOs;
using Entity.Model;
using Microsoft.Extensions.Logging;
using Utilities.Exceptions;

namespace Business
{
    public class ModuleBusiness
    {
        private readonly ModuleData<SqlServerDbContext> _sqlModuleData;
        private readonly ModuleData<PostgresDbContext> _pgModuleData;
        private readonly ModuleData<MySqlDbContext> _myModuleData;
        private readonly ILogger<ModuleBusiness> _logger;

        public ModuleBusiness(
            ModuleData<SqlServerDbContext> sqlModuleData,
            ModuleData<PostgresDbContext> pgModuleData,
            ModuleData<MySqlDbContext> myModuleData,
            ILogger<ModuleBusiness> logger)
        {
            _sqlModuleData = sqlModuleData;
            _pgModuleData = pgModuleData;
            _myModuleData = myModuleData;
            _logger = logger;
        }

        public async Task<IEnumerable<ModuleDto>> GetAllModuleAsync()
        {
            try
            {
                var modules = await _sqlModuleData.GetAllAsync();
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
                var module = await _sqlModuleData.GetByIdAsync(id);
                if (module == null)
                    throw new EntityNotFoundException("Module", id);

                return MapToDto(module);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el módulo con ID: {ModuleId}", id);
                throw;
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
                var sqlModule = await _sqlModuleData.CreateAsync(module);
                await _pgModuleData.CreateAsync(module);
                await _myModuleData.CreateAsync(module);

                return MapToDto(sqlModule);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear módulo: {ModuleName}", moduleDto?.Name);
                throw;
            }
        }

        public async Task<ModuleDto> UpdateModuleAsync(ModuleDto moduleDto)
        {
            if (moduleDto?.Id <= 0)
                throw new ValidationException("Id", "El ID del módulo debe ser mayor que cero");

            ValidateModule(moduleDto);

            try
            {
                var module = MapToEntity(moduleDto);

                // Actualizar en todas las bases de datos
                var updateResult = await _sqlModuleData.UpdateAsync(module);
                await _pgModuleData.UpdateAsync(module);
                await _myModuleData.UpdateAsync(module);

                if (!updateResult)
                    throw new EntityNotFoundException("Module", moduleDto.Id);

                var updatedModule = await _sqlModuleData.GetByIdAsync(moduleDto.Id);
                return MapToDto(updatedModule);
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
                await _sqlModuleData.DeleteAsync(id);
                await _pgModuleData.DeleteAsync(id);
                await _myModuleData.DeleteAsync(id);
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
                await _sqlModuleData.DisableAsync(id);
                await _pgModuleData.DisableAsync(id);
                await _myModuleData.DisableAsync(id);
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
                var existingModule = await _sqlModuleData.GetByIdAsync(moduleDto.Id);
                if (existingModule == null)
                    throw new EntityNotFoundException($"No se encontró el módulo con ID {moduleDto.Id}");

                if (!string.IsNullOrEmpty(moduleDto.Name))
                    existingModule.Name = moduleDto.Name;
                existingModule.Active = moduleDto.Active;

                // Actualizar parcialmente en todas las bases de datos
                await _sqlModuleData.PartialUpdateModuleAsync(existingModule,
                    nameof(existingModule.Name),
                    nameof(existingModule.Active));
                await _pgModuleData.PartialUpdateModuleAsync(existingModule,
                    nameof(existingModule.Name),
                    nameof(existingModule.Active));
                await _myModuleData.PartialUpdateModuleAsync(existingModule,
                    nameof(existingModule.Name),
                    nameof(existingModule.Active));
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