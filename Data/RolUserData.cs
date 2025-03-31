using Entity.Context;
using Entity.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Data
{
  
    public class RolUserData
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        ///<summary>
        ///Constructor que recibe el contexto de la base de datos
        ///</summary>
        ///<param name="context">Instancia de <see cref="ApplicationDbContext"/>para la conexion con la base de datos</param>
        public RolUserData(ApplicationDbContext context, ILogger logger)
        {
            _context = context;
            _logger = logger;
        }

        ///<summary>
        ///Crea un nuevo RolUser en la base de datos
        ///</summary>
        ///<param name="rolUser">Instancia del RolUser a crear</param>
        ///<returns>El RolUser creado</returns>
        public async Task<RolUser> CreateAsync(RolUser rolUser)
        {
            try
            {
                await _context.Set<RolUser>().AddAsync(rolUser);
                await _context.SaveChangesAsync();
                return rolUser;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al crear el RolUser: {ex.Message}");
                throw;
            }
        }

        ///<summary>
        ///Obtiene todos los RolUsers almacenados en la base de datos
        ///</summary>
        ///<returns>Lista de los RolUsers</returns>
        public async Task<IEnumerable<RolUser>> GetAllAsync()
        {
            return await _context.Set<RolUser>().ToListAsync();
        }
        ///<summary>Obtiene un RolUser especifico por su identificador</summary>
        public async Task<RolUser?> GestByIdAsync(int id)
        {
            try
            {
                return await _context.Set<RolUser>().FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener RolUser con ID {RolUserId}", id);
                throw;
            }
        }
        ///<summary>
        ///Actualiza un RolUser existente en la base de datos
        ///</summary>
        ///<param name="rolUser">Objeto con la informacion actualizada</param>
        ///<returns>True si la operacion fue exitosa, false en caso contrario</returns>
        public async Task<bool> UpdateAsync(RolUser rolUser)
        {
            try
            {
                _context.Set<RolUser>().Update(rolUser);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al actualizar el RolUser: {ex.Message}");
                return false;
            }
        }
        ///<summary>
        ///Elimina un RolUser de la base de datos
        ///</summary>
        ///<param name="id">Identificador unico del RolUser a eliminar</param>
        ///<returns>True si la eliminacion fue exitosa, false en caso contrario</returns>
        ///
        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var rolUser = await _context.Set<RolUser>().FindAsync(id);
                if (rolUser == null)
                    return false;

                _context.Set<RolUser>().Remove(rolUser);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al elminar el RolUser: {ex.Message}");
                return false;
            }
        }
    }
}
