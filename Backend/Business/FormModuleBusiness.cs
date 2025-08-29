using Business.Services;
using Entity.DTOs;
using Entity.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Utilities.Exceptions;

namespace Business
{
    public class FormModuleBusiness
    {
        private readonly MultiDatabaseService _multiDbService;
        private readonly ILogger<FormModuleBusiness> _logger;

        public FormModuleBusiness(
            MultiDatabaseService multiDbService,
            ILogger<FormModuleBusiness> logger)
        {
            _multiDbService = multiDbService;
            _logger = logger;
        }

        public async Task<IEnumerable<FormModuleDto>> GetAllFormModulesAsync()
        {
            try
            {
                var formModules = await _multiDbService.ExecuteWithFailoverAsync(async (context) =>
                {
                    return await context.Set<FormModule>().ToListAsync();
                });
                return formModules.Select(MapToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los formModules");
                throw new ExternalServiceException("Base de datos", "Error al recuperar la lista de formModules", ex);
            }
        }

        public async Task<FormModuleDto> GetFormModuleByIdAsync(int id)
        {
            if (id <= 0)
                throw new ValidationException("id", "El ID del formModule debe ser mayor que cero");

            try
            {
                var formModule = await _multiDbService.ExecuteWithFailoverAsync(async (context) =>
                {
                    return await context.Set<FormModule>().FirstOrDefaultAsync(fm => fm.Id == id);
                });

                if (formModule == null)
                    throw new EntityNotFoundException("FormModule", id);

                return MapToDto(formModule);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el formModule con ID: {FormModuleId}", id);
                throw new ExternalServiceException("Base de datos", $"Error al recuperar el formModule con ID {id}", ex);
            }
        }

        public async Task<FormModuleDto> CreateFormModuleAsync(FormModuleDto formModuleDto)
        {
            try
            {
                ValidateFormModule(formModuleDto);
                var formModule = MapToEntity(formModuleDto);

                // Crear en todas las bases de datos
                await _multiDbService.ExecuteInAllDatabasesAsync(async (context) =>
                {
                    context.Set<FormModule>().Add(formModule);
                    await context.SaveChangesAsync();
                });

                return MapToDto(formModule);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear formModule");
                throw new ExternalServiceException("Base de datos", "Error al crear el formModule", ex);
            }
        }

        public async Task UpdateFormModuleAsync(FormModuleDto formModuleDto)
        {
            if (formModuleDto.Id <= 0)
                throw new ValidationException("id", "El ID del formModule debe ser mayor que cero");

            ValidateFormModule(formModuleDto);

            try
            {
                // Verificar que existe usando failover
                var existing = await _multiDbService.ExecuteWithFailoverAsync(async (context) =>
                {
                    return await context.Set<FormModule>().FirstOrDefaultAsync(fm => fm.Id == formModuleDto.Id);
                });

                if (existing == null)
                    throw new EntityNotFoundException("FormModule", formModuleDto.Id);

                var entityToUpdate = MapToEntity(formModuleDto);

                // Actualizar en todas las bases de datos
                await _multiDbService.ExecuteInAllDatabasesAsync(async (context) =>
                {
                    context.Set<FormModule>().Update(entityToUpdate);
                    await context.SaveChangesAsync();
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar formModule con ID: {FormModuleId}", formModuleDto.Id);
                throw;
            }
        }

        public async Task DeleteFormModuleAsync(int id)
        {
            if (id <= 0)
                throw new ValidationException("id", "El ID del formModule debe ser mayor que cero");

            try
            {
                // Eliminar de todas las bases de datos
                await _multiDbService.ExecuteInAllDatabasesAsync(async (context) =>
                {
                    var formModule = await context.Set<FormModule>().FirstOrDefaultAsync(fm => fm.Id == id);
                    if (formModule != null)
                    {
                        context.Set<FormModule>().Remove(formModule);
                        await context.SaveChangesAsync();
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar formModule con ID: {FormModuleId}", id);
                throw;
            }
        }

        private void ValidateFormModule(FormModuleDto formModuleDto)
        {
            if (formModuleDto == null)
                throw new ValidationException("El objeto formModule no puede ser nulo");

            if (formModuleDto.FormId <= 0)
                throw new ValidationException("FormId", "El FormId del formModule es obligatorio");

            if (formModuleDto.ModuleId <= 0)
                throw new ValidationException("ModuleId", "El ModuleId del formModule es obligatorio");
        }

        private FormModuleDto MapToDto(FormModule formModule)
        {
            return new FormModuleDto
            {
                Id = formModule.Id,
                ModuleId = formModule.ModuleId,
                FormId = formModule.FormId
            };
        }

        private FormModule MapToEntity(FormModuleDto dto)
        {
            return new FormModule
            {
                Id = dto.Id,
                ModuleId = dto.ModuleId,
                FormId = dto.FormId
            };
        }
    }
}