using Entity.Context;
using Entity.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Data
{
    /// <summary>
    /// Repositorio genérico encargado de la gestión de la entidad Permission en cualquier base de datos
    /// </summary>
    public class PermissionData<TContext> where TContext : ApplicationDbContext<TContext>
    {
        private readonly TContext _context;
        private readonly ILogger<PermissionData<TContext>> _logger;

        public PermissionData(TContext context, ILogger<PermissionData<TContext>> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Permission> CreateAsync(Permission permission)
        {
            try
            {
                await _context.Set<Permission>().AddAsync(permission);
                await _context.SaveChangesAsync();
                return permission;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al crear el permiso: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<Permission>> GetAllAsync()
        {
            return await _context.Set<Permission>()
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Permission?> GetByIdAsync(int id)
        {
            try
            {
                return await _context.Set<Permission>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener permiso con ID {PermissionId}", id);
                throw;
            }
        }

        public async Task<bool> UpdateAsync(Permission permission)
        {
            try
            {
                _context.Set<Permission>().Update(permission);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el permiso con ID {PermissionId}", permission.Id);
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var permission = await _context.Set<Permission>().FindAsync(id);
                if (permission == null)
                    return false;

                _context.Set<Permission>().Remove(permission);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el permiso con ID {PermissionId}", id);
                return false;
            }
        }

        public async Task<bool> DisableAsync(int id)
        {
            try
            {
                var permission = await _context.Set<Permission>().FindAsync(id);
                if (permission == null)
                    return false;

                permission.Active = false;
                _context.Set<Permission>().Update(permission);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al realizar eliminación lógica del permiso con ID {PermissionId}", id);
                return false;
            }
        }

        public async Task PartialUpdatePermissionAsync(Permission permission, params string[] propertiesToUpdate)
        {
            try
            {
                var entry = _context.Entry(permission);
                foreach (var property in propertiesToUpdate)
                {
                    entry.Property(property).IsModified = true;
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar parcialmente el permiso con ID {PermissionId}", permission.Id);
                throw;
            }
        }
    }
}