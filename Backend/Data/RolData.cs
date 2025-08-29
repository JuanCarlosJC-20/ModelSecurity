using Entity.Context;
using Entity.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Data
{
    /// <summary>
    /// Repositorio genérico encargado de la gestión de la entidad Rol en cualquier base de datos
    /// </summary>
    public class RolData<TContext> where TContext : ApplicationDbContext<TContext>
    {
        private readonly IDbContextFactory<TContext> _contextFactory;
        private readonly ILogger<RolData<TContext>> _logger;

        public RolData(IDbContextFactory<TContext> contextFactory, ILogger<RolData<TContext>> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        public async Task<Rol> CreateAsync(Rol rol)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                await context.Set<Rol>().AddAsync(rol);
                await context.SaveChangesAsync();
                return rol;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al crear el rol: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<Rol>> GetAllAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Set<Rol>()
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Rol?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Set<Rol>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener rol con ID {RolId}", id);
                throw;
            }
        }

        public async Task<bool> UpdateAsync(Rol rol)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                context.Set<Rol>().Update(rol);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el rol con ID {RolId}", rol.Id);
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var rol = await context.Set<Rol>().FindAsync(id);
                if (rol == null)
                    return false;

                context.Set<Rol>().Remove(rol);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el rol con ID {RolId}", id);
                return false;
            }
        }

        public async Task<bool> DisableAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var rol = await context.Set<Rol>().FindAsync(id);
                if (rol == null)
                    return false;

                rol.Active = false;
                context.Set<Rol>().Update(rol);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al realizar eliminación lógica del rol con ID {RolId}", id);
                return false;
            }
        }

        public async Task PartialUpdateRolAsync(Rol rol, params string[] propertiesToUpdate)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var entry = context.Entry(rol);
                foreach (var property in propertiesToUpdate)
                {
                    entry.Property(property).IsModified = true;
                }
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar parcialmente el rol con ID {RolId}", rol.Id);
                throw;
            }
        }
    }
}