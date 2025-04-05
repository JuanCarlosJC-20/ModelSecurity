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
        private readonly ILogger _logger;

        public FormModuleBusiness(FormModuleData formModuleData, ILogger logger)
        {
            _formModuleData = formModuleData;
            _logger = logger;
        }

        // Método para obtener todos los formModules como DTOs
        public async Task<IEnumerable<FormModuleDto>> GetAllFormModulesAsync()
        {
            try
            {
                var formModules = await _formModuleData.GetAllAsync();
                var formModulesDtos = new List<FormModuleDto>();

                foreach (var formModule in formModules)
                {
                    formModulesDtos.Add(new FormModuleDto
                    {
                        Id = formModule.Id,
                        ModuleId = formModule.ModuleId,
                        FormId = formModule.FormId
                    });
                }

                return formModulesDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los formModules");
                throw new ExternalServiceException("Base de datos", "Error al recuperar la lista de formModules", ex);
            }
        }

        // Método para obtener un formModule por ID como DTO
        public async Task<FormModuleDto> GetFormModuleByIdAsync(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Se intentó obtener un formModule con ID inválido: {FormModuleId}", id);
                throw new Utilities.Exceptions.ValidationException("id", "El ID del formModule debe ser mayor que cero");
            }

            try
            {
                var formModule = await _formModuleData.GetByIdAsync(id);
                if (formModule == null)
                {
                    _logger.LogInformation("No se encontró ningún formModule con ID: {FormModuleId}", id);
                    throw new EntityNotFoundException("FormModule", id);
                }

                return new FormModuleDto
                {
                    Id = formModule.Id,
                    ModuleId = formModule.ModuleId,
                    FormId = formModule.FormId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el formModule con ID: {FormModuleId}", id);
                throw new ExternalServiceException("Base de datos", $"Error al recuperar el formModule con ID {id}", ex);
            }
        }

        // Método para crear un formModule desde un DTO
        public async Task<FormModuleDto> CreateFormModuleAsync(FormModuleDto formModuleDto)
        {
            try
            {
                ValidateFormModule(formModuleDto);

                var formModule = new FormModule
                {
                    FormId = formModuleDto.FormId,
                    ModuleId = formModuleDto.ModuleId // Si existe en la entidad
                };

                var formModuleCreado = await _formModuleData.CreateAsync(formModule);

                return new FormModuleDto
                {
                    Id = formModuleCreado.Id,
                    FormId = formModuleCreado.FormId,
                    ModuleId = formModuleCreado.ModuleId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear nuevo formModule: {FormModuleFormId}" ,formModuleDto.FormId <= 0);
                _logger.LogError(ex, "Error al crear nuevo formModule: {FormModuleModuleId}", formModuleDto.ModuleId <= 0);
                throw new ExternalServiceException("Base de datos", "Error al crear el formModule", ex);
            }
        }

        // Método para validar el DTO
        private void ValidateFormModule(FormModuleDto formModuleDto)
        {
            if (formModuleDto == null)
            {
                throw new Utilities.Exceptions.ValidationException("El objeto formModule no puede ser nulo");
            }

            if (formModuleDto.FormId <= 0)
            {
                _logger.LogWarning("Se intentó crear/actualizar un formModule con FormId vacío");
                throw new Utilities.Exceptions.ValidationException("FormId", "El FormId del formModule es obligatorio");
            }
            if (formModuleDto.ModuleId <= 0)
            {
                _logger.LogWarning("Se intentó crear/actualizar un formModule con ModuleId vacío");
                throw new Utilities.Exceptions.ValidationException("ModuleId", "El ModuleId del formModule es obligatorio");
            }
        }
    }
}