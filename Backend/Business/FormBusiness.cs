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
    public class FormBusiness
    {
        private readonly MultiDatabaseService _multiDbService;
        private readonly ILogger<FormBusiness> _logger;

        public FormBusiness(
            MultiDatabaseService multiDbService,
            ILogger<FormBusiness> logger)
        {
            _multiDbService = multiDbService;
            _logger = logger;
        }

        public async Task<IEnumerable<FormDto>> GetAllFormsAsync()
        {
            try
            {
                // Obtener con failover automático
                var forms = await _multiDbService.ExecuteWithFailoverAsync(async (context) =>
                {
                    return await context.Set<Form>()
                        .Where(f => f.Active)
                        .ToListAsync();
                });
                
                return forms.Select(MapToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los formularios");
                throw new ExternalServiceException("Base de datos", "Error al recuperar la lista de formularios", ex);
            }
        }

        public async Task<FormDto> GetFormByIdAsync(int id)
        {
            if (id <= 0)
                throw new ValidationException("id", "El ID del formulario debe ser mayor que cero");

            try
            {
                var form = await _multiDbService.ExecuteWithFailoverAsync(async (context) =>
                {
                    return await context.Set<Form>()
                        .FirstOrDefaultAsync(f => f.Id == id);
                });
                
                if (form == null)
                    throw new EntityNotFoundException("Form", id);

                return MapToDto(form);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el formulario con ID: {FormId}", id);
                throw new ExternalServiceException("Base de datos", $"Error al recuperar el formulario con ID {id}", ex);
            }
        }

        public async Task<FormDto> CreateFormAsync(FormDto formDto)
        {
            try
            {
                ValidateForm(formDto);
                var form = new Form
                {
                    Name = formDto.Name,
                    Code = formDto.Code,
                    Active = formDto.Active,
                    CreateAt = DateTime.UtcNow
                };

                // Crear en todas las bases de datos
                var createdForm = await _multiDbService.ExecuteInAllDatabasesAsync(async (context) =>
                {
                    context.Set<Form>().Add(form);
                    await context.SaveChangesAsync();
                    return form;
                });

                return MapToDto(form);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear formulario: {FormNombre}", formDto?.Name ?? "null");
                throw new ExternalServiceException("Base de datos", "Error al crear el formulario", ex);
            }
        }

        public async Task UpdateFormAsync(FormDto formDto)
        {
            if (formDto == null || formDto.Id <= 0)
                throw new ValidationException("Id", "El formulario a actualizar debe tener un ID válido");

            ValidateForm(formDto);

            try
            {
                // Obtener existente con failover
                var existing = await _multiDbService.ExecuteWithFailoverAsync(async (context) =>
                {
                    return await context.Set<Form>()
                        .FirstOrDefaultAsync(f => f.Id == formDto.Id);
                });
                
                if (existing == null)
                    throw new EntityNotFoundException("Form", formDto.Id);

                // Actualizar en todas las bases de datos
                await _multiDbService.ExecuteInAllDatabasesAsync(async (context) =>
                {
                    var formToUpdate = await context.Set<Form>()
                        .FirstOrDefaultAsync(f => f.Id == formDto.Id);
                    
                    if (formToUpdate != null)
                    {
                        formToUpdate.Name = formDto.Name;
                        formToUpdate.Code = formDto.Code;
                        formToUpdate.Active = formDto.Active;
                        await context.SaveChangesAsync();
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar formulario con ID: {FormId}", formDto.Id);
                throw;
            }
        }

        public async Task DeleteFormAsync(int id)
        {
            if (id <= 0)
                throw new ValidationException("id", "El ID del formulario debe ser mayor que cero");

            try
            {
                // Eliminar de todas las bases de datos
                await _multiDbService.ExecuteInAllDatabasesAsync(async (context) =>
                {
                    var formToDelete = await context.Set<Form>()
                        .FirstOrDefaultAsync(f => f.Id == id);
                    
                    if (formToDelete != null)
                    {
                        context.Set<Form>().Remove(formToDelete);
                        await context.SaveChangesAsync();
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar formulario con ID: {FormId}", id);
                throw;
            }
        }

        public async Task DisableFormAsync(int id)
        {
            if (id <= 0)
                throw new ValidationException("id", "El ID del formulario debe ser mayor que cero");

            try
            {
                // Deshabilitar en todas las bases de datos
                await _multiDbService.ExecuteInAllDatabasesAsync(async (context) =>
                {
                    var formToDisable = await context.Set<Form>()
                        .FirstOrDefaultAsync(f => f.Id == id);
                    
                    if (formToDisable != null)
                    {
                        formToDisable.Active = false;
                        formToDisable.DeleteAt = DateTime.UtcNow;
                        await context.SaveChangesAsync();
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al desactivar formulario con ID: {FormId}", id);
                throw;
            }
        }

        public async Task PartialUpdateFormAsync(FormDto formDto)
        {
            // Obtener existente con failover
            var existingForm = await _multiDbService.ExecuteWithFailoverAsync(async (context) =>
            {
                return await context.Set<Form>()
                    .FirstOrDefaultAsync(f => f.Id == formDto.Id);
            });
            
            if (existingForm == null)
            {
                throw new EntityNotFoundException($"No se encontró el formulario con ID {formDto.Id}.");
            }

            // Actualizar parcialmente en todas las bases de datos
            await _multiDbService.ExecuteInAllDatabasesAsync(async (context) =>
            {
                var formToUpdate = await context.Set<Form>()
                    .FirstOrDefaultAsync(f => f.Id == formDto.Id);
                
                if (formToUpdate != null)
                {
                    if (!string.IsNullOrEmpty(formDto.Name))
                        formToUpdate.Name = formDto.Name;
                    if (!string.IsNullOrEmpty(formDto.Code))
                        formToUpdate.Code = formDto.Code;
                    formToUpdate.Active = formDto.Active;
                    
                    await context.SaveChangesAsync();
                }
            });
        }

        private void ValidateForm(FormDto formDto)
        {
            if (formDto == null)
                throw new ValidationException("formDto", "El objeto formulario no puede ser nulo");

            if (string.IsNullOrWhiteSpace(formDto.Name))
                throw new ValidationException("Name", "El Name del formulario es obligatorio");
        }

        private FormDto MapToDto(Form form)
        {
            return new FormDto
            {
                Id = form.Id,
                Name = form.Name,
                Code = form.Code,
                Active = form.Active
            };
        }
    }
}