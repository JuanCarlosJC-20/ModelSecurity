using Entity.Context;
using Entity.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
namespace Data
{
    /// <summary>
    /// Repositorio encargado de la gestion de la entidad FormModule en la base de datos
    /// </summary>
    public class FormModuleData
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FormModuleData> _logger;

        ///<summary>
        ///Constructor que recibe el contexto de la base de datos
        ///</summary>
        ///<param name="context">Instancia de <see cref="ApplicationDbContext"/>para la conexion con la base de datos</param>
        public FormModuleData(ApplicationDbContext context, ILogger<FormModuleData> logger)
        {
            _context = context;
            _logger = logger;
        }

        ///<summary>
        ///Crea un nuevo FormModule en la base de datos
        ///</summary>
        ///<param name="formModule">Instancia del formModule a crear</param>
        ///<returns>El formModule creado</returns>
        public async Task<FormModule> CreateAsync(FormModule formModule)
        {
            try
            {
                await _context.Set<FormModule>().AddAsync(formModule);
                await _context.SaveChangesAsync();
                return formModule;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al crear el formModule: {ex.Message}");
                throw;
            }
        }

        ///<summary>
        ///Obtiene todos los formModules almacenados en la base de datos
        ///</summary>
        ///<returns>Lista de los formModules</returns>
        public async Task<IEnumerable<FormModule>> GetAllAsync()
        {
            return await _context.Set<FormModule>().ToListAsync();
        }
        ///<summary>Obtiene un FormModule especifico por su identificador</summary>
        public async Task<FormModule?> GetByIdAsync(int id)
        {
            try
            {
                return await _context.Set<FormModule>().FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener FormModule con ID {FormModuleId}", id);
                throw;
            }
        }
        ///<summary>
        ///Actualiza un FormModule existente en la base de datos
        ///</summary>
        ///<param name="formModule">Objeto con la informacion actualizada</param>
        ///<returns>True si la operacion fue exitosa, false en caso contrario</returns>
        public async Task<bool> UpdateAsync(FormModule formModule)
        {
            try
            {
                _context.Set<FormModule>().Update(formModule);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al actualizar el formModule: {ex.Message}");
                return false;
            }
        }
        ///<summary>
        ///Elimina un formModule de la base de datos
        ///</summary>
        ///<param name="id">Identificador unico del formModule a eliminar</param>
        ///<returns>True si la eliminacion fue exitosa, false en caso contrario</returns>
        ///
        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var formModule = await _context.Set<FormModule>().FindAsync(id);
                if (formModule == null)
                    return false;

                _context.Set<FormModule>().Remove(formModule);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al elminar el formModule: {ex.Message}");
                return false;
            }
        }


    }
}
