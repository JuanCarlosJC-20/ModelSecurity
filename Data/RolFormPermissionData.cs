using Entity.Context;
using Entity.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Data
{
    /// <summary>
    /// Repositorio encargado de la gestion de la entidad RolFormPermission en la base de datos
    /// </summary>
    public class RolFormPermissionData
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RolFormPermissionData> _logger;

        ///<summary>
        ///Constructor que recibe el contexto de la base de datos
        ///</summary>
        ///<param name="context">Instancia de <see cref="ApplicationDbContext"/>para la conexion con la base de datos</param>
        public RolFormPermissionData(ApplicationDbContext context, ILogger<RolFormPermissionData> logger)
        {
            _context = context;
            _logger = logger;
        }

        ///<summary>
        ///Crea un nuevo RolFormPermission en la base de datos
        ///</summary>
        ///<param name="rolFormPermission">Instancia del rolFormPermission a crear</param>
        ///<returns>El rolFormPermission creado</returns>
        public async Task<RolFormPermission> CreateAsync(RolFormPermission rolFormPermission)
        {
            try
            {
                await _context.Set<RolFormPermission>().AddAsync(rolFormPermission);
                await _context.SaveChangesAsync();
                return rolFormPermission;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al crear el rolFormPermission: {ex.Message}");
                throw;
            }
        }

        ///<summary>
        ///Obtiene todos los rolFormPermissions almacenados en la base de datos
        ///</summary>
        ///<returns>Lista de los rolFormPermissions</returns>
        public async Task<IEnumerable<RolFormPermission>> GetAllAsync()
        {
            return await _context.Set<RolFormPermission>().ToListAsync();
        }
        ///<summary>Obtiene un RolFormPermission especifico por su identificador</summary>
        public async Task<RolFormPermission?> GetByIdAsync(int id)
        {
            try
            {
                return await _context.Set<RolFormPermission>().FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener RolFormPermission con ID {RolFormPermissionId}", id);
                throw;
            }
        }
        ///<summary>
        ///Actualiza un RolFormPermission existente en la base de datos
        ///</summary>
        ///<param name="rolFormPermission">Objeto con la informacion actualizada</param>
        ///<returns>True si la operacion fue exitosa, false en caso contrario</returns>
        public async Task<bool> UpdateAsync(RolFormPermission rolFormPermission)
        {
            try
            {
                _context.Set<RolFormPermission>().Update(rolFormPermission);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al actualizar el rolFormPermission: {ex.Message}");
                return false;
            }
        }
        ///<summary>
        ///Elimina un rolFormPermission de la base de datos
        ///</summary>
        ///<param name="id">Identificador unico del rolFormPermission a eliminar</param>
        ///<returns>True si la eliminacion fue exitosa, false en caso contrario</returns>
        ///
        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var rolFormPermission = await _context.Set<RolFormPermission>().FindAsync(id);
                if (rolFormPermission == null)
                    return false;

                _context.Set<RolFormPermission>().Remove(rolFormPermission);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al elminar el rolFormPermission: {ex.Message}");
                return false;
            }
        }
    }
}
