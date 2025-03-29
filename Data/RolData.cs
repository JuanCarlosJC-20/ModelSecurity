namespace Data
{
    public class RolData
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RolData> _logger;

        public RolData(ApplicationDbContext context, ILogger<RolData> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Crea un nuevo rol en la base de datos.
        /// </summary>
        /// <param name="rol">Instancia del rol a crear.</param>
        /// <returns>El rol creado.</returns>
        public async Task<Rol> CreateAsync(Rol rol)
        {
            try
            {
                await _context.Set<Rol>().AddAsync(rol);
                await _context.SaveChangesAsync();
                return rol;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al crear el rol: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Obtiene todos los roles almacenados en la base de datos.
        /// </summary>
        /// <returns>Lista de roles.</returns>
        public async Task<IEnumerable<Rol>> GetAllAsync()
        {
            return await _context.Set<Rol>().ToListAsync();
        }

        /// <summary>
        /// Obtiene un rol específico por su identificador.
        /// </summary>
        /// <param name="id">Identificador único del rol.</param>
        /// <returns>El rol encontrado o null si no existe.</returns>
        public async Task<Rol?> GetByIdAsync(int id)
        {
            try
            {
                return await _context.Set<Rol>().FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener rol con ID {RolId}", id);
                throw; // Re-lanza la excepción para que sea manejada en capas superiores
            }
        }

        /// <summary>
        /// Actualiza un rol existente en la base de datos.
        /// </summary>
        /// <param name="rol">Objeto con la información actualizada.</param>
        /// <returns>True si la operación fue exitosa, False en caso contrario.</returns>
        public async Task<bool> UpdateAsync(Rol rol)
        {
            try
            {
                _context.Set<Rol>().Update(rol);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al actualizar el rol: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Elimina un rol de la base de datos.
        /// </summary>
        /// <param name="id">Identificador único del rol a eliminar.</param>
        /// <returns>True si la eliminación fue exitosa, False en caso contrario.</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var rol = await _context.Set<Rol>().FindAsync(id);
                if (rol == null)
                    return false;

                _context.Set<Rol>().Remove(rol);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al eliminar el rol: {ex.Message}");
                return false;
            }
        }
    }
}
