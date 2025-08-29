using Entity.Context;
using Entity.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Data
{
    /// <summary>
    /// Repositorio genérico encargado de la gestión de la entidad User en cualquier base de datos
    /// </summary>
    public class UserData<TContext> where TContext : ApplicationDbContext<TContext>
    {
        private readonly IDbContextFactory<TContext> _contextFactory;
        private readonly ILogger<UserData<TContext>> _logger;

        public UserData(IDbContextFactory<TContext> contextFactory, ILogger<UserData<TContext>> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        public async Task<User> CreateAsync(User user)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                await context.Set<User>().AddAsync(user);
                await context.SaveChangesAsync();
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al crear el usuario: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Set<User>()
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Set<User>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario con ID {UserId}", id);
                throw;
            }
        }

        public async Task<bool> UpdateAsync(User user)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                context.Set<User>().Update(user);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el usuario con ID {UserId}", user.Id);
                return false;
            }
        }

        public async Task<bool> PatchAsync(int userId, User user)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var existingUser = await context.Set<User>().FindAsync(userId);
                if (existingUser == null)
                {
                    _logger.LogError($"Usuario con ID {userId} no encontrado.");
                    return false;
                }

                if (user.PersonId != 0)
                    existingUser.PersonId = user.PersonId;
                if (!string.IsNullOrEmpty(user.UserName))
                    existingUser.UserName = user.UserName;
                if (!string.IsNullOrEmpty(user.PasswordHash))
                    existingUser.PasswordHash = user.PasswordHash;
                if (!string.IsNullOrEmpty(user.Code))
                    existingUser.Code = user.Code;
                if (user.Active != null)
                    existingUser.Active = user.Active;

                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al actualizar parcialmente el usuario con ID {userId}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DisableAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var user = await context.Set<User>().FindAsync(id);
                if (user == null)
                    return false;

                user.Active = false;
                user.DeleteAt = DateTime.Now;
                context.Set<User>().Update(user);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al desactivar usuario con ID {UserId}", id);
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var user = await context.Set<User>().FindAsync(id);
                if (user == null)
                    return false;

                context.Set<User>().Remove(user);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar usuario con ID {UserId}", id);
                return false;
            }
        }
    }
}