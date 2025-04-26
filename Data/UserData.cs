using Entity.Context;
using Entity.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Data
{    /// <summary>
     /// Repositorio encargado de la gestion de la entidad User en la base de datos
     /// </summary>
    public class UserData
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserData> _logger;

        ///<summary>
        ///Constructor que recibe el contexto de la base de datos
        ///</summary>
        ///<param name="context">Instancia de <see cref="ApplicationDbContext"/>para la conexion con la base de datos</param>
        public UserData(ApplicationDbContext context, ILogger<UserData> logger)
        {
            _context = context;
            _logger = logger;
        }

        ///<summary>
        ///Crea un nuevo usuario en la base de datos
        ///</summary>
        ///<param name="user">Instancia del usuario a crear</param>
        ///<returns>El usuario creado</returns>
        public async Task<User> CreateAsync(User user)
        {
            try
            {
                await _context.Set<User>().AddAsync(user);
                await _context.SaveChangesAsync();
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al crear el usuario: {ex.Message}");
                throw;
            }
        }

        ///<summary>
        ///Obtiene todos los usuarios almacenados en la base de datos
        ///</summary>
        ///<returns>Lista de los usuarios</returns>
        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Set<User>().ToListAsync();
        }
        ///<summary>Obtiene un usuario especifico por su identificador</summary>
        public async Task<User?> GetByIdAsync(int id)
        {
            try
            {
                return await _context.Set<User>().FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario con ID {UserId}", id);
                throw;
            }
        }
        ///<summary>
        ///Actualiza un usuario existente en la base de datos
        ///</summary>
        ///<param name="user">Objeto con la informacion actualizada</param>
        ///<returns>True si la operacion fue exitosa, false en caso contrario</returns>
        public async Task<bool> UpdateAsync(User user)
        {
            try
            {
                _context.Set<User>().Update(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al actualizar el usuario: {ex.Message}");
                return false;
            }
        }
        ///<summary>
        ///Elimina un usuario de la base de datos
        ///</summary>
        ///<param name="id">Identificador unico del usuario a eliminar</param>
        ///<returns>True si la eliminacion fue exitosa, false en caso contrario</returns>
        ///
        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var user = await _context.Set<User>().FindAsync(id);
                if (user == null)
                    return false;

                _context.Set<User>().Remove(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al elminar el usuario: {ex.Message}");
                return false;
            }
        }

                    /// <summary>
    /// Realiza una eliminación lógica del user, marcándolo como inactivo.
    /// </summary>
    /// <param name="id">ID del user a desactivar</param>
    /// <returns>True si se desactivó correctamente, false si no se encontró</returns>
    public async Task<bool> DisableAsync(int id)
    {
        try
        {
            var user = await _context.Set<User>().FindAsync(id);
            if (user == null)
                return false;

            user.Active = false;
            _context.Set<User>().Update(user);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al realizar eliminación lógica del user con ID {userId}", id);
            return false;
        }
    }
    }
}
