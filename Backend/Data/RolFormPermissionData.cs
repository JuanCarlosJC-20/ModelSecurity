using Entity.Context;
using Entity.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Data
{
    /// <summary>
    /// Repositorio genérico encargado de la gestión de la entidad RolFormPermission en cualquier base de datos
    /// </summary>
    public class RolFormPermissionData<TContext> where TContext : ApplicationDbContext<TContext>
    {
        private readonly IDbContextFactory<TContext> _contextFactory;
        private readonly ILogger<RolFormPermissionData<TContext>> _logger;

        public RolFormPermissionData(IDbContextFactory<TContext> contextFactory, ILogger<RolFormPermissionData<TContext>> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        public async Task<RolFormPermission> CreateAsync(RolFormPermission rolFormPermission)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                await context.Set<RolFormPermission>().AddAsync(rolFormPermission);
                await context.SaveChangesAsync();
                return rolFormPermission;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al crear el rolFormPermission: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<RolFormPermission>> GetAllAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Set<RolFormPermission>()
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<RolFormPermission?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Set<RolFormPermission>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener RolFormPermission con ID {RolFormPermissionId}", id);
                throw;
            }
        }

        public async Task<bool> UpdateAsync(RolFormPermission rolFormPermission)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                context.Set<RolFormPermission>().Update(rolFormPermission);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el RolFormPermission con ID {RolFormPermissionId}", rolFormPermission.Id);
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var rolFormPermission = await context.Set<RolFormPermission>().FindAsync(id);
                if (rolFormPermission == null)
                    return false;

                context.Set<RolFormPermission>().Remove(rolFormPermission);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el RolFormPermission con ID {RolFormPermissionId}", id);
                return false;
            }
        }
    }
}