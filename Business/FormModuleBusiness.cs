using Data;
using Entity.DTOs;
using Entity.Model;
using Microsoft.Extensions.Logging;
using Utilities.Exceptions;

namespace Business
{
    /// <summary>
    /// Clase de negocio encargada de la lógica relacionada con los formModules del sistema.
    /// </summary>
    public class FormModuleBusiness
    {
        private readonly FormModuleData _formModuleData;
        private readonly ILogger<FormModuleBusiness> _logger;

        public FormModuleBusiness(FormModuleData formModuleData, ILogger<FormModuleBusiness> logger)
        {
            _formModuleData = formModuleData;
            _logger = logger;
        }

        public async Task<IEnumerable<FormModuleDto>> GetAllFormModulesAsync()
        {
            try
            {
                var formModules = await _formModuleData.GetAllAsync();
                return MapToDtoList(formModules);
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
            {
                _logger.LogWarning("Se intentó obtener un formModule con ID inválido: {FormModuleId}", id);
                throw new ValidationException("id", "El ID del formModule debe ser mayor que cero");
            }

            try
            {
                var formModule = await _formModuleData.GetByIdAsync(id);
                if (formModule == null)
                {
                    _logger.LogInformation("No se encontró ningún formModule con ID: {FormModuleId}", id);
                    throw new EntityNotFoundException("FormModule", id);
                }

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
                var created = await _formModuleData.CreateAsync(formModule);

                return MapToDto(created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear nuevo formModule");
                throw new ExternalServiceException("Base de datos", "Error al crear el formModule", ex);
            }
        }

        public async Task<FormModuleDto> UpdateFormModuleAsync(FormModuleDto formModuleDto)
        {
            try
            {
                if (formModuleDto.Id <= 0)
                {
                    throw new ValidationException("id", "El ID del formModule debe ser mayor que cero");
                }

                ValidateFormModule(formModuleDto);

                var existing = await _formModuleData.GetByIdAsync(formModuleDto.Id);
                if (existing == null)
                {
                    throw new EntityNotFoundException("FormModule", formModuleDto.Id);
                }

                var entityToUpdate = MapToEntity(formModuleDto);
                var updated = await _formModuleData.UpdateAsync(entityToUpdate);

                return MapToDto(updated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar formModule");
                throw new ExternalServiceException("Base de datos", "Error al actualizar el formModule", ex);
            }
        }

        private FormModuleDto MapToDto(bool updated)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteFormModuleAsync(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new ValidationException("id", "El ID del formModule debe ser mayor que cero");
                }

                var existing = await _formModuleData.GetByIdAsync(id);
                if (existing == null)
                {
                    throw new EntityNotFoundException("FormModule", id);
                }

                await _formModuleData.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar formModule con ID: {FormModuleId}", id);
                throw new ExternalServiceException("Base de datos", $"Error al eliminar el formModule con ID {id}", ex);
            }
        }

        private void ValidateFormModule(FormModuleDto formModuleDto)
        {
            if (formModuleDto == null)
            {
                throw new ValidationException("El objeto formModule no puede ser nulo");
            }

            if (formModuleDto.FormId <= 0)
            {
                throw new ValidationException("FormId", "El FormId del formModule es obligatorio");
            }

            if (formModuleDto.ModuleId <= 0)
            {
                throw new ValidationException("ModuleId", "El ModuleId del formModule es obligatorio");
            }
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

        private IEnumerable<FormModuleDto> MapToDtoList(IEnumerable<FormModule> entities)
        {
            return entities.Select(MapToDto).ToList();
        }



       

    }
}
