using Data;
using Entity.DTOs;
using Entity.Model;
using Microsoft.Extensions.Logging;
using Utilities.Exceptions;

namespace Business
{
    /// <summary>
    /// Clase de negocio encargada de la lógica relacionada con los formularios del sistema.
    /// </summary>
    public class FormBusiness
    {
        private readonly FormData _formData;
        private readonly ILogger<FormBusiness> _logger;

        public FormBusiness(FormData formData, ILogger<FormBusiness> logger)
        {
            _formData = formData;
            _logger = logger;
        }

        public async Task<IEnumerable<FormDto>> GetAllFormsAsync()
        {
            try
            {

                var forms = await _formData.GetAllAsync();
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
                var form = await _formData.GetByIdAsync(id);
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
                    Active = formDto.Active
                };

                
                 form.CreateAt=DateTime.Now;

                var formCreado = await _formData.CreateAsync(form);
                return MapToDto(formCreado);
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
                var existing = await _formData.GetByIdAsync(formDto.Id);
                if (existing == null)
                    throw new EntityNotFoundException("Form", formDto.Id);

                existing.Name = formDto.Name;
                existing.Code = formDto.Code;
                existing.Active = formDto.Active;

                var result = await _formData.UpdateAsync(existing);
                if (!result)
                    throw new ExternalServiceException("Base de datos", "Error al actualizar el formulario");
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
                var existing = await _formData.GetByIdAsync(id);
                if (existing == null)
                    throw new EntityNotFoundException("Form", id);

                var result = await _formData.DeleteAsync(id);
                if (!result)
                    throw new ExternalServiceException("Base de datos", "No se pudo eliminar el formulario");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar formulario con ID: {FormId}", id);
                throw;
            }
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
