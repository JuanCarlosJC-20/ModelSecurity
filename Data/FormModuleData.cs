using Entity.Context;
using Entity.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Data
{
    /// <summary>
    /// Repositorio genérico encargado de la gestión de la entidad FormModule en cualquier base de datos
    /// </summary>
    public class FormModuleData<TContext> where TContext : ApplicationDbContext<TContext>
    {
        private readonly TContext _context;
        private readonly ILogger<FormModuleData<TContext>> _logger;

        public FormModuleData(TContext context, ILogger<FormModuleData<TContext>> logger)
        {
            _context = context;
            _logger = logger;
        }

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

        public async Task<IEnumerable<FormModule>> GetAllAsync()
        {
            return await _context.Set<FormModule>()
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<FormModule?> GetByIdAsync(int id)
        {
            try
            {
                return await _context.Set<FormModule>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(f => f.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener FormModule con ID {FormModuleId}", id);
                throw;
            }
        }

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
                _logger.LogError(ex, "Error al actualizar el formModule con ID {FormModuleId}", formModule.Id);
                return false;
            }
        }

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
                _logger.LogError(ex, "Error al eliminar el formModule con ID {FormModuleId}", id);
                return false;
            }
        }
    }
}