using Data;
using Entity.Context;
using Entity.DTOs;
using Entity.Model;
using Microsoft.Extensions.Logging;
using Utilities.Exceptions;

namespace Business
{
    public class PersonBusiness
    {
        private readonly PersonData<SqlServerDbContext> _sqlPersonData;
        private readonly PersonData<PostgresDbContext> _pgPersonData;
        private readonly PersonData<MySqlDbContext> _myPersonData;
        private readonly ILogger<PersonBusiness> _logger;

        public PersonBusiness(
            PersonData<SqlServerDbContext> sqlPersonData,
            PersonData<PostgresDbContext> pgPersonData,
            PersonData<MySqlDbContext> myPersonData,
            ILogger<PersonBusiness> logger)
        {
            _sqlPersonData = sqlPersonData;
            _pgPersonData = pgPersonData;
            _myPersonData = myPersonData;
            _logger = logger;
        }

        public async Task<IEnumerable<PersonDto>> GetAllPersonAsync()
        {
            try
            {
                var persons = await _sqlPersonData.GetAllAsync();
                return MapToDtoList(persons);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todas las personas");
                throw new ExternalServiceException("Base de datos", "Error al recuperar la lista de personas", ex);
            }
        }

        public async Task<PersonDto> GetPersonByIdAsync(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("ID inválido al buscar persona: {PersonId}", id);
                throw new ValidationException("id", "El ID de la persona debe ser mayor que cero");
            }

            var person = await _sqlPersonData.GetByIdAsync(id);
            if (person == null)
                throw new EntityNotFoundException("Person", id);

            return MapToDto(person);
        }

        public async Task<PersonDto> CreatePersonAsync(PersonDto personDto)
        {
            try
            {
                ValidatePerson(personDto);
                var person = MapToEntity(personDto);

                // Crear en todas las bases de datos
                var sqlPerson = await _sqlPersonData.CreateAsync(person);
                await _pgPersonData.CreateAsync(person);
                await _myPersonData.CreateAsync(person);

                return MapToDto(sqlPerson);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear persona: {PersonName}", $"{personDto?.FirstName} {personDto?.LastName}");
                throw;
            }
        }

        public async Task<PersonDto> UpdatePersonAsync(int id, PersonDto personDto)
        {
            if (id <= 0 || personDto == null)
                throw new ValidationException("id", "El ID de la persona debe ser mayor que cero");

            ValidatePerson(personDto);

            try
            {
                var person = MapToEntity(personDto);

                // Actualizar en todas las bases de datos
                await _sqlPersonData.UpdateAsync(person);
                await _pgPersonData.UpdateAsync(person);
                await _myPersonData.UpdateAsync(person);

                return MapToDto(person);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar persona con ID: {PersonId}", id);
                throw;
            }
        }

        public async Task DeletePersonAsync(int id)
        {
            if (id <= 0)
                throw new ValidationException("id", "El ID de la persona debe ser mayor que cero");

            try
            {
                // Eliminar de todas las bases de datos
                await _sqlPersonData.DeleteAsync(id);
                await _pgPersonData.DeleteAsync(id);
                await _myPersonData.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar persona con ID: {PersonId}", id);
                throw;
            }
        }

        public async Task PartialUpdatePersonAsync(PersonDto personDto)
        {
            try
            {
                var existingPerson = await _sqlPersonData.GetByIdAsync(personDto.Id);
                if (existingPerson == null)
                    throw new EntityNotFoundException("Person", personDto.Id);

                if (!string.IsNullOrEmpty(personDto.FirstName))
                    existingPerson.FirstName = personDto.FirstName;
                if (!string.IsNullOrEmpty(personDto.LastName))
                    existingPerson.LastName = personDto.LastName;
                if (!string.IsNullOrEmpty(personDto.Email))
                    existingPerson.Email = personDto.Email;

                // Actualizar parcialmente en todas las bases de datos
                await _sqlPersonData.PartialUpdateAsync(existingPerson,
                    nameof(existingPerson.FirstName),
                    nameof(existingPerson.LastName),
                    nameof(existingPerson.Email));
                await _pgPersonData.PartialUpdateAsync(existingPerson,
                    nameof(existingPerson.FirstName),
                    nameof(existingPerson.LastName),
                    nameof(existingPerson.Email));
                await _myPersonData.PartialUpdateAsync(existingPerson,
                    nameof(existingPerson.FirstName),
                    nameof(existingPerson.LastName),
                    nameof(existingPerson.Email));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar parcialmente la persona con ID: {PersonId}", personDto.Id);
                throw;
            }
        }

        private void ValidatePerson(PersonDto personDto)
        {
            if (personDto == null)
                throw new ValidationException("El objeto persona no puede ser nulo");

            if (string.IsNullOrWhiteSpace(personDto.FirstName))
                throw new ValidationException("FirstName", "El nombre de la persona es obligatorio");

            if (string.IsNullOrWhiteSpace(personDto.LastName))
                throw new ValidationException("LastName", "El apellido de la persona es obligatorio");

            if (string.IsNullOrWhiteSpace(personDto.Email))
                throw new ValidationException("Email", "El email de la persona es obligatorio");
        }

        private PersonDto MapToDto(Person person) => new()
        {
            Id = person.Id,
            FirstName = person.FirstName,
            LastName = person.LastName,
            Email = person.Email
        };

        private Person MapToEntity(PersonDto dto) => new()
        {
            Id = dto.Id,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email
        };

        private IEnumerable<PersonDto> MapToDtoList(IEnumerable<Person> persons)
            => persons.Select(MapToDto);
    }
}
