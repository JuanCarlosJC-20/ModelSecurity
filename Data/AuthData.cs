using Entity.Context;
using Entity.Model;              // Ajusta si tu entidad User está en otro namespace
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Data
{
    /// <summary>
    /// Acceso a datos para autenticación
    /// </summary>
    public class AuthData
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuthData> _logger;

        public AuthData(ApplicationDbContext context, ILogger<AuthData> logger)
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
                return await _context.Set<User>()
                    // Incluye relaciones si las necesitas para Claims (Roles, Persona, etc.)
                    // .Include(u => u.RolUsers).ThenInclude(ru => ru.Rol)
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
        /// (Opcional) Obtiene un usuario por Id, útil si luego quieres refrescar claims.
        /// </summary>
        public async Task<User?> GetByIdAsync(int id)
        {
            try
            {
                return await _context.Set<User>()
                    // .Include(u => u.RolUsers).ThenInclude(ru => ru.Rol)
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
        /// (Opcional) Registra fecha/hora del último login.
        /// Solo si tu entidad User tiene ese campo (ajústalo o elimina este método).
        /// </summary>
        public async Task<bool> UpdateLastLoginAsync(int userId, DateTime when)
        {
            try
            {
                var user = await _context.Set<User>().FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null) return false;

                // Si tienes un campo LastLoginAt en User, descomenta/ajusta:
                // user.LastLoginAt = when;

                _context.Set<User>().Update(user);
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
