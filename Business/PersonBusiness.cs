using Data;
using Entity.DTOs;
using Entity.Model;
using Microsoft.Extensions.Logging;
using System;
using Utilities.Exceptions;

namespace Business
{
    /// <summary>
    /// Clase de negocio encargada de la lógica relacionada con las personas del sistema.
    /// </summary>
    public class PersonBusiness
    {
        private readonly PersonData _personData;
        private readonly ILogger<PersonBusiness> _logger;

        public PersonBusiness(PersonData personData, ILogger<PersonBusiness> logger)
        {
            _personData = personData;
            _logger = logger;
        }

        // Obtener todas las personas
        public async Task<IEnumerable<PersonDto>> GetAllPersonAsync()
        {
            try
            {
                var persons = await _personData.GetAllAsync();
                return MapToDtoList(persons);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todas las personas");
                throw new ExternalServiceException("Base de datos", "Error al recuperar la lista de personas", ex);
            }
        }

        // Obtener persona por ID
        public async Task<PersonDto> GetPersonByIdAsync(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("ID inválido al buscar persona: {PersonId}", id);
                throw new ValidationException("id", "El ID de la persona debe ser mayor que cero");
            }

            try
            {
                var person = await _personData.GetByIdAsync(id);
                if (person == null)
                {
                    _logger.LogInformation("Persona no encontrada con ID: {PersonId}", id);
                    throw new EntityNotFoundException("Person", id);
                }

                return MapToDto(person);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener persona con ID: {PersonId}", id);
                throw new ExternalServiceException("Base de datos", $"Error al recuperar la persona con ID {id}", ex);
            }
        }

        // Crear persona
        public async Task<PersonDto> CreatePersonAsync(PersonDto personDto)
        {
            try
            {
                ValidatePerson(personDto);

                var entity = MapToEntity(personDto);
                var created = await _personData.CreateAsync(entity);

                return MapToDto(created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear nueva persona: {FirstName}", personDto?.FirstName ?? "null");
                throw new ExternalServiceException("Base de datos", "Error al crear la persona", ex);
            }
        }

        // Actualizar persona
        public async Task<PersonDto> UpdatePersonAsync(int id, PersonDto personDto)
        {
            if (personDto == null || personDto.Id <= 0)
            {
                throw new ValidationException("id", "El ID de la persona debe ser mayor que cero");
            }

            try
            {
                ValidatePerson(personDto);

                var existing = await _personData.GetByIdAsync(personDto.Id);
                if (existing == null)
                {
                    _logger.LogInformation("Persona no encontrada para actualizar: {PersonId}", personDto.Id);
                    throw new EntityNotFoundException("Person", personDto.Id);
                }

                var updated = await _personData.UpdateAsync(MapToEntity(personDto));
                return MapToDto(updated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar persona con ID: {PersonId}", personDto.Id);
                throw new ExternalServiceException("Base de datos", "Error al actualizar la persona", ex);
            }
        }

        private PersonDto MapToDto(bool updated)
        {
            throw new NotImplementedException();
        }

        // Eliminar persona
        public async Task DeletePersonAsync(int id)
        {
            if (id <= 0)
            {
                throw new ValidationException("id", "El ID de la persona debe ser mayor que cero");
            }

            try
            {
                var existing = await _personData.GetByIdAsync(id);
                if (existing == null)
                {
                    _logger.LogInformation("Persona no encontrada para eliminar con ID: {PersonId}", id);
                    throw new EntityNotFoundException("Person", id);
                }

                await _personData.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar persona con ID: {PersonId}", id);
                throw new ExternalServiceException("Base de datos", "Error al eliminar la persona", ex);
            }
        }

        // Validación
        private void ValidatePerson(PersonDto personDto)
        {
            if (personDto == null)
            {
                throw new ValidationException("El objeto persona no puede ser nulo");
            }

            if (string.IsNullOrWhiteSpace(personDto.FirstName))
            {
                _logger.LogWarning("FirstName vacío al crear/actualizar persona");
                throw new ValidationException("FirstName", "El FirstName de la persona es obligatorio");
            }

            if (string.IsNullOrWhiteSpace(personDto.LastName))
            {
                _logger.LogWarning("LastName vacío al crear/actualizar persona");
                throw new ValidationException("LastName", "El LastName de la persona es obligatorio");
            }

            if (string.IsNullOrWhiteSpace(personDto.Email))
            {
                _logger.LogWarning("Email vacío al crear/actualizar persona");
                throw new ValidationException("Email", "El Email de la persona es obligatorio");
            }
        }

        // Mapeos
        private PersonDto MapToDto(Person person)
        {
            return new PersonDto
            {
                Id = person.Id,
                FirstName = person.FirstName,
                LastName = person.LastName,
                Email = person.Email
            };
        }

        private Person MapToEntity(PersonDto dto)
        {
            return new Person
            {
                Id = dto.Id,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email
            };
        }

        private IEnumerable<PersonDto> MapToDtoList(IEnumerable<Person> persons)
        {
            return persons.Select(MapToDto).ToList();
        }
    }
}
