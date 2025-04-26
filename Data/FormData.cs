using Entity.Context;
using Entity.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Data
{    
    /// <summary>
     /// Repositorio encargado de la gestion de la entidad Form en la base de datos
     /// </summary>
    public class FormData 
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FormData> _logger;
        ///<summary>
        ///Constructor que recibe el contexto de la base de datos
        ///</summary>
        ///<param name="context">Instancia de <see cref="ApplicationDbContext"/>para la conexion con la base de datos</param>
        public FormData(ApplicationDbContext context, ILogger<FormData> logger)
        {
            _context = context;
            _logger = logger;
        }

        ///<summary>
        ///Crea un nuevo formulario en la base de datos
        ///</summary>
        ///<param name="form">Instancia del formulario a crear</param>
        ///<returns>El formulario creado</returns>
        public async Task<Form> CreateAsync(Form form)
        {
            try
            {
                await _context.Set<Form>().AddAsync(form);
                await _context.SaveChangesAsync();
                return form;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al crear el formulario: {ex.Message}");
                throw;
            }
        }

        ///<summary>
        ///Obtiene todos los formularios almacenados en la base de datos
        ///</summary>
        ///<returns>Lista de los formularios</returnsa>
        public async Task<IEnumerable<Form>> GetAllAsync()
        {
            return await _context.Set<Form>().ToListAsync();
        }
        ///<summary>Obtiene un formulario especifico por su identificador</summary>
        public async Task<Form?> GetByIdAsync(int id)
        {
            try
            {
                return await _context.Set<Form>().FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener formulario con ID {FormId}", id);
                throw;
            }
        }
        ///<summary>
        ///Actualiza un formulario existente en la base de datos
        ///</summary>
        ///<param name="form">Objeto con la informacion actualizada</param>
        ///<returns>True si la operacion fue exitosa, false en caso contrario</returns>
        public async Task<bool> UpdateAsync(Form form)
        {
            try
            {
                _context.Set<Form>().Update(form);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al actualizar el formulario: {ex.Message}");
                return false;
            }
        }
        ///<summary>
        ///Elimina un formulario de la base de datos
        ///</summary>
        ///<param name="id">Identificador unico del formulario a eliminar</param>
        ///<returns>True si la eliminacion fue exitosa, false en caso contrario</returns>
        ///
        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var form = await _context.Set<Form>().FindAsync(id);
                if (form == null)
                    return false;

                _context.Set<Form>().Remove(form);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al elminar el formulario: {ex.Message}");
                return false;
            }

        }


         /// <summary>
    /// Realiza una eliminación lógica del formulario, marcándolo como inactivo.
    /// </summary>
    /// <param name="id">ID del formulario a desactivar</param>
    /// <returns>True si se desactivó correctamente, false si no se encontró</returns>
    public async Task<bool> DisableAsync(int id)
    {
        try
        {
            var form = await _context.Set<Form>().FindAsync(id);
            if (form == null)
                return false;

            form.Active = false;
            _context.Set<Form>().Update(form);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al realizar eliminación lógica del formulario con ID {FormId}", id);
            return false;
        }
    }


//datos de patch para actualizar parcialmente un formulario
    public async Task PartialUpdateFormAsync(Form form, params string[] propertiesToUpdate)
{
    var entry = _context.Entry(form);

    foreach (var property in propertiesToUpdate)
    {
        entry.Property(property).IsModified = true;
    }

    await _context.SaveChangesAsync();
}

    }
}
