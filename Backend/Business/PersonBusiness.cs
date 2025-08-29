using Business.Services;
using Entity.DTOs;
using Entity.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Utilities.Exceptions;

namespace Business
{
    public class PersonBusiness
    {
        private readonly MultiDatabaseService _multiDbService;
        private readonly ILogger<PersonBusiness> _logger;

        public PersonBusiness(
            MultiDatabaseService multiDbService,
            ILogger<PersonBusiness> logger)
        {
            _multiDbService = multiDbService;
            _logger = logger;
        }

        public async Task<IEnumerable<PersonDto>> GetAllPersonAsync()
        {
            try
            {
                var persons = await _multiDbService.ExecuteWithFailoverAsync(async (context) =>
                {
                    return await context.Set<Person>().ToListAsync();
                });
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

            var person = await _multiDbService.ExecuteWithFailoverAsync(async (context) =>
            {
                return await context.Set<Person>().FirstOrDefaultAsync(p => p.Id == id);
            });

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
                await _multiDbService.ExecuteInAllDatabasesAsync(async (context) =>
                {
                    context.Set<Person>().Add(person);
                    await context.SaveChangesAsync();
                });

                return MapToDto(person);
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
                person.Id = id;

                // Actualizar en todas las bases de datos
                await _multiDbService.ExecuteInAllDatabasesAsync(async (context) =>
                {
                    context.Set<Person>().Update(person);
                    await context.SaveChangesAsync();
                });

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
                await _multiDbService.ExecuteInAllDatabasesAsync(async (context) =>
                {
                    var person = await context.Set<Person>().FirstOrDefaultAsync(p => p.Id == id);
                    if (person != null)
                    {
                        context.Set<Person>().Remove(person);
                        await context.SaveChangesAsync();
                    }
                });
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
                // Obtener persona existente usando failover
                var existingPerson = await _multiDbService.ExecuteWithFailoverAsync(async (context) =>
                {
                    return await context.Set<Person>().FirstOrDefaultAsync(p => p.Id == personDto.Id);
                });

                if (existingPerson == null)
                    throw new EntityNotFoundException("Person", personDto.Id);

                if (!string.IsNullOrEmpty(personDto.FirstName))
                    existingPerson.FirstName = personDto.FirstName;
                if (!string.IsNullOrEmpty(personDto.LastName))
                    existingPerson.LastName = personDto.LastName;
                if (!string.IsNullOrEmpty(personDto.Email))
                    existingPerson.Email = personDto.Email;

                // Actualizar parcialmente en todas las bases de datos
                await _multiDbService.ExecuteInAllDatabasesAsync(async (context) =>
                {
                    var person = await context.Set<Person>().FirstOrDefaultAsync(p => p.Id == personDto.Id);
                    if (person != null)
                    {
                        if (!string.IsNullOrEmpty(personDto.FirstName))
                            person.FirstName = personDto.FirstName;
                        if (!string.IsNullOrEmpty(personDto.LastName))
                            person.LastName = personDto.LastName;
                        if (!string.IsNullOrEmpty(personDto.Email))
                            person.Email = personDto.Email;
                        
                        await context.SaveChangesAsync();
                    }
                });
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
