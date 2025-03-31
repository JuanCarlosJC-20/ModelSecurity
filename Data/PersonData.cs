using Entity.Context;
using Entity.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Data
{

    public class PersonData
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        ///<summary>
        ///Constructor que recibe el contexto de la base de datos
        ///</summary>
        ///<param name="context">Instancia de <see cref="ApplicationDbContext"/>para la conexion con la base de datos</param>
        public PersonData(ApplicationDbContext context, ILogger logger)
        {
            _context = context;
            _logger = logger;
        }

        ///<summary>
        ///Crea una nueva persona en la base de datos
        ///</summary>
        ///<param name="person">Instancia de la persona a crear</param>
        ///<returns>La persona creada</returns>
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
                _logger.LogError($"Error al crear la persona: {ex.Message}");
                throw;
            }
        }

        ///<summary>
        ///Obtiene todos las personas almacenadas en la base de datos
        ///</summary>
        ///<returns>Lista de las personas</returns>
        public async Task<IEnumerable<Person>> GetAllAsync()
        {
            return await _context.Set<Person>().ToListAsync();
        }
        ///<summary>Obtiene una persona especifico por su identificador</summary>
        public async Task<Person?> GetByIdAsync(int id)
        {
            try
            {
                return await _context.Set<Person>().FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener personas con ID {PersonId}", id);
                throw;
            }
        }
        ///<summary>
        ///Actualiza una persona existente en la base de datos
        ///</summary>
        ///<param name="person">Objeto con la informacion actualizada</param>
        ///<returns>True si la operacion fue exitosa, false en caso contrario</returns>
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
                _logger.LogError($"Error al actualizar la persona: {ex.Message}");
                return false;
            }
        }
        ///<summary>
        ///Elimina una persona de la base de datos
        ///</summary>
        ///<param name="id">Identificador unico de la persona a eliminar</param>
        ///<returns>True si la eliminacion fue exitosa, false en caso contrario</returns>
        ///
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
                Console.WriteLine($"Error al elminar la persona: {ex.Message}");
                return false;
            }
        }
    }
}
