using Entity.Context;
using Entity.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Data
{
    /// <summary>
    /// Repositorio genérico encargado de la gestión de la entidad RolUser en cualquier base de datos
    /// </summary>
    public class RolUserData<TContext> where TContext : ApplicationDbContext<TContext>
    {
        private readonly IDbContextFactory<TContext> _contextFactory;
        private readonly ILogger<RolUserData<TContext>> _logger;

        public RolUserData(IDbContextFactory<TContext> contextFactory, ILogger<RolUserData<TContext>> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        public async Task<RolUser> CreateAsync(RolUser rolUser)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                await context.Set<RolUser>().AddAsync(rolUser);
                await context.SaveChangesAsync();
                return rolUser;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al crear el RolUser: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<RolUser>> GetAllAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Set<RolUser>()
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<RolUser?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Set<RolUser>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener RolUser con ID {RolUserId}", id);
                throw;
            }
        }

        public async Task<bool> UpdateAsync(RolUser rolUser)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                context.Set<RolUser>().Update(rolUser);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el RolUser con ID {RolUserId}", rolUser.Id);
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var rolUser = await context.Set<RolUser>().FindAsync(id);
                if (rolUser == null)
                    return false;

                context.Set<RolUser>().Remove(rolUser);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el RolUser con ID {RolUserId}", id);
                return false;
            }
        }
    }
}