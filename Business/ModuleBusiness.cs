using Data;
using Entity.DTOs;
using Entity.Model;
using Microsoft.Extensions.Logging;
using Utilities.Exceptions;

namespace Business
{
    /// <summary>
    /// Clase de negocio encargada de la lógica relacionada con los módulos del sistema.
    /// </summary>
    public class ModuleBusiness
    {
        private readonly ModuleData _moduleData;
        private readonly ILogger<ModuleBusiness> _logger;

        public ModuleBusiness(ModuleData moduleData, ILogger<ModuleBusiness> logger)
        {
            _moduleData = moduleData;
            _logger = logger;
        }

        // Método para obtener todos los módulos como DTOs
        public async Task<IEnumerable<ModuleDto>> GetAllModuleAsync()
        {
            try
            {
                var modules = await _moduleData.GetAllAsync();
                return MapToDtoList(modules);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los módulos");
                throw new ExternalServiceException("Base de datos", "Error al recuperar la lista de módulos", ex);
            }
        }

        // Método para obtener un módulo por ID como DTO
        public async Task<ModuleDto> GetModuleByIdAsync(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Se intentó obtener un módulo con ID inválido: {ModuleId}", id);
                throw new ValidationException("id", "El ID del módulo debe ser mayor que cero");
            }

            try
            {
                var module = await _moduleData.GetByIdAsync(id);
                if (module == null)
                {
                    _logger.LogInformation("No se encontró ningún módulo con ID: {ModuleId}", id);
                    throw new EntityNotFoundException("Modulo", id);
                }

                return MapToDto(module);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el módulo con ID: {ModuleId}", id);
                throw new ExternalServiceException("Base de datos", $"Error al recuperar el módulo con ID {id}", ex);
            }
        }

        // Método para crear un módulo desde un DTO
        public async Task<ModuleDto> CreateModuleAsync(ModuleDto moduleDto)
        {
            try
            {
                ValidateModule(moduleDto);

                var module = MapToEntity(moduleDto);
                 module.CreateAt=DateTime.Now;
                var createdModule = await _moduleData.CreateAsync(module);

                return MapToDto(createdModule);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear nuevo módulo: {ModuleNombre}", moduleDto?.Name ?? "null");
                throw new ExternalServiceException("Base de datos", "Error al crear el módulo", ex);
            }
        }

        // Método para actualizar un módulo existente
        public async Task<ModuleDto> UpdateModuleAsync(ModuleDto moduleDto)
        {
            try
            {
                if (moduleDto == null || moduleDto.Id <= 0)
                {
                    throw new ValidationException("Id", "El ID del módulo debe ser mayor que cero");
                }

                ValidateModule(moduleDto);

                var existingModule = await _moduleData.GetByIdAsync(moduleDto.Id);
                if (existingModule == null)
                {
                    _logger.LogInformation("No se encontró módulo para actualizar con ID: {ModuleId}", moduleDto.Id);
                    throw new EntityNotFoundException("Modulo", moduleDto.Id);
                }

                var updatedModule = await _moduleData.UpdateAsync(MapToEntity(moduleDto));
                return MapToDto(updatedModule);
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (EntityNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar módulo con ID: {ModuleId}", moduleDto?.Id ?? 0);
                throw new ExternalServiceException("Base de datos", "Error al actualizar el módulo", ex);
            }
        }

        private ModuleDto MapToDto(bool updatedModule)
        {
            throw new NotImplementedException();
        }

        // Método para eliminar un módulo por su ID
        public async Task DeleteModuleAsync(int id)
        {
            if (id <= 0)
            {
                throw new ValidationException("id", "El ID del módulo debe ser mayor que cero");
            }

            try
            {
                var existingModule = await _moduleData.GetByIdAsync(id);
                if (existingModule == null)
                {
                    _logger.LogInformation("No se encontró módulo para eliminar con ID: {ModuleId}", id);
                    throw new EntityNotFoundException("Modulo", id);
                }

                await _moduleData.DeleteAsync(id);
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (EntityNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar módulo con ID: {ModuleId}", id);
                throw new ExternalServiceException("Base de datos", "Error al eliminar el módulo", ex);
            }
        }

        // Método para validar el DTO
        private void ValidateModule(ModuleDto moduleDto)
        {
            if (moduleDto == null)
            {
                throw new ValidationException("El objeto módulo no puede ser nulo");
            }

            if (string.IsNullOrWhiteSpace(moduleDto.Name))
            {
                _logger.LogWarning("Se intentó crear/actualizar un módulo con Name vacío");
                throw new ValidationException("Name", "El Name del módulo es obligatorio");
            }
        }

        // Método para mapear un entity a DTO
        private ModuleDto MapToDto(Module module)
        {
            return new ModuleDto
            {
                Id = module.Id,
                Name = module.Name,
                Active = module.Active
            };
        }

        // Método para mapear un DTO a entity
        private Module MapToEntity(ModuleDto dto)
        {
            return new Module
            {
                Id = dto.Id,
                Name = dto.Name,
                Active = dto.Active
            };
        }

        // Método para mapear una lista de entidades a DTOs
        private IEnumerable<ModuleDto> MapToDtoList(IEnumerable<Module> modules)
        {
            var moduleDtos = new List<ModuleDto>();
            foreach (var module in modules)
            {
                moduleDtos.Add(MapToDto(module));
            }
            return moduleDtos;
        }

         /// <summary>
/// Realiza una eliminación lógica del module.
/// </summary>
/// <param name="id">ID del module</param>
public async Task DisableFormAsync(int id)
{
    if (id <= 0)
        throw new ValidationException("id", "El ID del module debe ser mayor que cero");

    try
    {
        var existing = await _moduleData.GetByIdAsync(id);
        if (existing == null)
            throw new EntityNotFoundException("Form", id);

        var result = await _moduleData.DisableAsync(id);
        if (!result)
            throw new ExternalServiceException("Base de datos", "No se pudo desactivar el module");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error al desactivar module con ID: {ModuleId}", id);
        throw;
    }
}

//metodo patch para actualizar solo el estado activo del formulario
public async Task PartialUpdateFormAsync(ModuleDto moduleDto)
{
    var existingForm = await _moduleData.GetByIdAsync(moduleDto.Id);
    if (existingForm == null)
    {
        throw new EntityNotFoundException($"No se encontró el permiso con ID {moduleDto.Id}.");
    }

    if (!string.IsNullOrEmpty(moduleDto.Name))
        existingForm.Name = moduleDto.Name;


    // Active es tipo bool, simplemente lo actualizamos.
    existingForm.Active = moduleDto.Active;

    await _moduleData.PartialUpdateFormAsync(existingForm,
        nameof(existingForm.Name),
        nameof(existingForm.Active));
}

        
    }
}
