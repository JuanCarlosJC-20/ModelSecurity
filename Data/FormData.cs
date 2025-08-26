using Entity.Context;
using Entity.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Data
{    
    /// <summary>
    /// Repositorio genérico encargado de la gestión de la entidad Form en cualquier base de datos
    /// </summary>
    public class FormData<TContext> where TContext : ApplicationDbContext<TContext>
    {
        private readonly TContext _context;
        private readonly ILogger<FormData<TContext>> _logger;

        public FormData(TContext context, ILogger<FormData<TContext>> logger)
        {
            _context = context;
            _logger = logger;
        }

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

        public async Task<IEnumerable<Form>> GetAllAsync()
        {
            return await _context.Set<Form>()
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Form?> GetByIdAsync(int id)
        {
            try
            {
                return await _context.Set<Form>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(f => f.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener formulario con ID {FormId}", id);
                throw;
            }
        }

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
                _logger.LogError(ex, "Error al actualizar el formulario con ID {FormId}", form.Id);
                return false;
            }
        }

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
                _logger.LogError(ex, "Error al eliminar el formulario con ID {FormId}", id);
                return false;
            }
        }

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

        public async Task PartialUpdateFormAsync(Form form, params string[] propertiesToUpdate)
        {
            try
            {
                var entry = _context.Entry(form);
                foreach (var property in propertiesToUpdate)
                {
                    entry.Property(property).IsModified = true;
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar parcialmente el formulario con ID {FormId}", form.Id);
                throw;
            }
        }
    }
}