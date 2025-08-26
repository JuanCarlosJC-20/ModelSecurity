using Entity.Context;
using Entity.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Data
{
    /// <summary>
    /// Repositorio genérico encargado de la gestión de la entidad Person en cualquier base de datos.
    /// </summary>
    public class PersonData<TContext> where TContext : ApplicationDbContext<TContext>
    {
        private readonly TContext _context;
        private readonly ILogger<PersonData<TContext>> _logger;

        public PersonData(TContext context, ILogger<PersonData<TContext>> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Crea una nueva persona en la base de datos.
        /// </summary>
        public async Task<Person> CreateAsync(Person person)
        {
            try
            {
                await _context.Set<Person>().AddAsync(person);
                await _context.SaveChangesAsync();
                return person;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear la persona");
                throw;
            }
        }

        /// <summary>
        /// Obtiene todas las personas.
        /// </summary>
        public async Task<IEnumerable<Person>> GetAllAsync()
        {
            return await _context.Set<Person>()
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene una persona por ID.
        /// </summary>
        public async Task<Person?> GetByIdAsync(int id)
        {
            try
            {
                return await _context.Set<Person>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener persona con ID {PersonId}", id);
                throw;
            }
        }

        /// <summary>
        /// Actualiza una persona existente.
        /// </summary>
        public async Task<bool> UpdateAsync(Person person)
        {
            try
            {
                _context.Set<Person>().Update(person);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar la persona con ID {PersonId}", person.Id);
                return false;
            }
        }

        /// <summary>
        /// Elimina físicamente una persona.
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var person = await _context.Set<Person>().FindAsync(id);
                if (person == null)
                    return false;

                _context.Set<Person>().Remove(person);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar persona con ID {PersonId}", id);
                return false;
            }
        }

        /// <summary>
        /// Eliminación lógica: desactiva la persona (requiere campo Active en Person).
        /// </summary>
        public async Task<bool> DisableAsync(int id)
        {
            try
            {
                var person = await _context.Set<Person>().FindAsync(id);
                if (person == null)
                    return false;

                person.Active = false; // ⚡ importante: marcar como inactivo
                _context.Set<Person>().Update(person);

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al desactivar persona con ID {PersonId}", id);
                return false;
            }
        }

        /// <summary>
        /// Actualización parcial de una persona.
        /// </summary>
        public async Task PartialUpdateAsync(Person person, params string[] propertiesToUpdate)
        {
            try
            {
                var entry = _context.Entry(person);
                foreach (var property in propertiesToUpdate)
                {
                    entry.Property(property).IsModified = true;
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar parcialmente persona con ID {PersonId}", person.Id);
                throw;
            }
        }
    }
}
