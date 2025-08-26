using Entity.Context;
using Entity.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Data
{
    /// <summary>
    /// Repositorio genérico encargado de la gestión de la entidad Module en cualquier base de datos
    /// </summary>
    public class ModuleData<TContext> where TContext : ApplicationDbContext<TContext>
    {
        private readonly TContext _context;
        private readonly ILogger<ModuleData<TContext>> _logger;

        public ModuleData(TContext context, ILogger<ModuleData<TContext>> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Module> CreateAsync(Module module)
        {
            try
            {
                await _context.Set<Module>().AddAsync(module);
                await _context.SaveChangesAsync();
                return module;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al crear el módulo: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<Module>> GetAllAsync()
        {
            return await _context.Set<Module>()
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Module?> GetByIdAsync(int id)
        {
            try
            {
                return await _context.Set<Module>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener módulo con ID {ModuleId}", id);
                throw;
            }
        }

        public async Task<bool> UpdateAsync(Module module)
        {
            try
            {
                _context.Set<Module>().Update(module);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el módulo con ID {ModuleId}", module.Id);
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var module = await _context.Set<Module>().FindAsync(id);
                if (module == null)
                    return false;

                _context.Set<Module>().Remove(module);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el módulo con ID {ModuleId}", id);
                return false;
            }
        }

        public async Task<bool> DisableAsync(int id)
        {
            try
            {
                var module = await _context.Set<Module>().FindAsync(id);
                if (module == null)
                    return false;

                module.Active = false;
                _context.Set<Module>().Update(module);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al realizar eliminación lógica del módulo con ID {ModuleId}", id);
                return false;
            }
        }

        public async Task PartialUpdateModuleAsync(Module module, params string[] propertiesToUpdate)
        {
            try
            {
                var entry = _context.Entry(module);
                foreach (var property in propertiesToUpdate)
                {
                    entry.Property(property).IsModified = true;
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar parcialmente el módulo con ID {ModuleId}", module.Id);
                throw;
            }
        }
    }
}