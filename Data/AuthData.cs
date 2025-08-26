using Entity.Context;
using Entity.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Data
{
    /// <summary>
    /// Acceso a datos para autenticación (multi-DbContext).
    /// </summary>
    public class AuthData<TContext> where TContext : ApplicationDbContext<TContext>
    {
        private readonly TContext _context;
        private readonly ILogger<AuthData<TContext>> _logger;

        public AuthData(TContext context, ILogger<AuthData<TContext>> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene un usuario activo por su UserName (sin validar contraseña).
        /// La validación del password déjala en AuthBusiness.
        /// </summary>
        public async Task<User?> GetByUserNameAsync(string userName)
        {
            try
            {
                return await _context.User
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.UserName == userName && (u.Active == null || u.Active == true));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario por UserName {UserName}", userName);
                throw;
            }
        }

        /// <summary>
        /// Obtiene un usuario por Id.
        /// </summary>
        public async Task<User?> GetByIdAsync(int id)
        {
            try
            {
                return await _context.User
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario con ID {UserId}", id);
                throw;
            }
        }

        /// <summary>
        /// Actualiza la fecha/hora del último login (si tu entidad User tiene ese campo).
        /// </summary>
        public async Task<bool> UpdateLastLoginAsync(int userId, DateTime when)
        {
            try
            {
                var user = await _context.User.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null) return false;

                // user.LastLoginAt = when; // si existe ese campo

                _context.User.Update(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar último login para el usuario {UserId}", userId);
                return false;
            }
        }
    }
}
