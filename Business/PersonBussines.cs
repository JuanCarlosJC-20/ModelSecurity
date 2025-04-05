using Data;
using Entity.DTOs;
using Entity.Model;
using Microsoft.Extensions.Logging;
using Utilities.Exceptions;

namespace Business
{
    /// <summary>
    /// Clase de negocio encargada de la lógica relacionada con las personas del sistema.
    /// </summary>
    public class PersonBusiness
    {
        private readonly PersonData _personData;
        private readonly ILogger _logger;

        public PersonBusiness(PersonData personData, ILogger logger)
        {
            _personData = personData;
            _logger = logger;
        }

        // Método para obtener todos las personas como DTOs
        public async Task<IEnumerable<PersonDTO>> GetAllPersonAsync()
        {
            try
            {
                var persons = await _personData.GetAllAsync();
                var personsDTO = new List<PersonDTO>();

                foreach (var person in persons)
                {
                    personsDTO.Add(new PersonDTO
                    {
                        Id = person.Id,
                        FirstName = person.FirstName,
                        LastName = person.LastName,
                        Email = person.Email
                    });
                }

                return personsDTO;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos las personas");
                throw new ExternalServiceException("Base de datos", "Error al recuperar la lista de personas", ex);
            }
        }

        // Método para obtener una persona por ID como DTO
        public async Task<PersonDTO> GetPersonByIdAsync(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Se intentó obtener una persona con ID inválido: {PersonId}", id);
                throw new Utilities.Exceptions.ValidationException("id", "El ID de la persona debe ser mayor que cero");
            }

            try
            {
                var person = await _personData.GetByIdAsync(id);
                if (person == null)
                {
                    _logger.LogInformation("No se encontró ningúna persona con ID: {PersonId}", id);
                    throw new EntityNotFoundException("Person", id);
                }

                return new PersonDTO
                {
                    Id = person.Id,
                    FirstName = person.FirstName,
                    LastName = person.LastName,
                    Email = person.Email
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la persona con ID: {PersonId}", id);
                throw new ExternalServiceException("Base de datos", $"Error al recuperar la persona con ID {id}", ex);
            }
        }

        // Método para crear una persona desde un DTO
        public async Task<PersonDTO> CreatePersonAsync(PersonDTO PersonDTO)
        {
            try
            {
                ValidatePerson(PersonDTO);

                var person = new Person
                {
                    FirstName = PersonDTO.FirstName,
                    LastName = PersonDTO.LastName,
                    Email = PersonDTO.Email
                };

                var personaCreada = await _personData.CreateAsync(person);

                return new PersonDTO
                {
                    Id = personaCreada.Id,
                    FirstName = personaCreada.FirstName,
                    LastName = personaCreada.LastName,
                    Email = personaCreada.Email
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear nueva persona: {PersonNombre}", PersonDTO?.FirstName ?? "null");
                throw new ExternalServiceException("Base de datos", "Error al crear la persona", ex);
            }
        }

        // Método para validar el DTO
        private void ValidatePerson(PersonDTO PersonDTO)
        {
            if (PersonDTO == null)
            {
                throw new Utilities.Exceptions.ValidationException("El objeto person no puede ser nulo");
            }

            if (string.IsNullOrWhiteSpace(PersonDTO.FirstName))
            {
                _logger.LogWarning("Se intentó crear/actualizar una persona con FirstName vacío");
                throw new Utilities.Exceptions.ValidationException("FirstName", "El FirstName de la persona es obligatorio");
            }
        }
    }
}