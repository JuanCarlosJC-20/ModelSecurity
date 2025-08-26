using Data;
using Entity.Context;
using Entity.DTOs;
using Entity.Model;
using Microsoft.Extensions.Logging;
using Utilities.Exceptions;

namespace Business
{
    public class FormBusiness
    {
        private readonly FormData<SqlServerDbContext> _sqlFormData;
        private readonly FormData<PostgresDbContext> _pgFormData;
        private readonly FormData<MySqlDbContext> _myFormData;
        private readonly ILogger<FormBusiness> _logger;

        public FormBusiness(
            FormData<SqlServerDbContext> sqlFormData,
            FormData<PostgresDbContext> pgFormData,
            FormData<MySqlDbContext> myFormData,
            ILogger<FormBusiness> logger)
        {
            _sqlFormData = sqlFormData;
            _pgFormData = pgFormData;
            _myFormData = myFormData;
            _logger = logger;
        }

        public async Task<IEnumerable<FormDto>> GetAllFormsAsync()
        {
            try
            {
                // Obtenemos de SQL Server por defecto
                var forms = await _sqlFormData.GetAllAsync();
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
                var form = await _sqlFormData.GetByIdAsync(id);
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
                var sqlForm = await _sqlFormData.CreateAsync(form);
                await _pgFormData.CreateAsync(form);
                await _myFormData.CreateAsync(form);

                return MapToDto(sqlForm);
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
                var existing = await _sqlFormData.GetByIdAsync(formDto.Id);
                if (existing == null)
                    throw new EntityNotFoundException("Form", formDto.Id);

                existing.Name = formDto.Name;
                existing.Code = formDto.Code;
                existing.Active = formDto.Active;

                // Actualizar en todas las bases de datos
                await _sqlFormData.UpdateAsync(existing);
                await _pgFormData.UpdateAsync(existing);
                await _myFormData.UpdateAsync(existing);
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
                await _sqlFormData.DeleteAsync(id);
                await _pgFormData.DeleteAsync(id);
                await _myFormData.DeleteAsync(id);
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
                await _sqlFormData.DisableAsync(id);
                await _pgFormData.DisableAsync(id);
                await _myFormData.DisableAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al desactivar formulario con ID: {FormId}", id);
                throw;
            }
        }

        public async Task PartialUpdateFormAsync(FormDto formDto)
        {
            var existingForm = await _sqlFormData.GetByIdAsync(formDto.Id);
            if (existingForm == null)
            {
                throw new EntityNotFoundException($"No se encontró el formulario con ID {formDto.Id}.");
            }

            if (!string.IsNullOrEmpty(formDto.Name))
                existingForm.Name = formDto.Name;
            if (!string.IsNullOrEmpty(formDto.Code))
                existingForm.Code = formDto.Code;
            existingForm.Active = formDto.Active;

            // Actualizar parcialmente en todas las bases de datos
            await _sqlFormData.PartialUpdateFormAsync(existingForm, 
                nameof(existingForm.Name), 
                nameof(existingForm.Code), 
                nameof(existingForm.Active));
            await _pgFormData.PartialUpdateFormAsync(existingForm, 
                nameof(existingForm.Name), 
                nameof(existingForm.Code), 
                nameof(existingForm.Active));
            await _myFormData.PartialUpdateFormAsync(existingForm, 
                nameof(existingForm.Name), 
                nameof(existingForm.Code), 
                nameof(existingForm.Active));
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