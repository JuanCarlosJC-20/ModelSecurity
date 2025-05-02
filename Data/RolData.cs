﻿using Entity.Context;
using Entity.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
namespace Data
{
    /// <summary>
    /// Repositorio encargado de la gestion de la entidad Rol en la base de datos
    /// </summary>
    public class RolData
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<Rol> _logger;

        ///<summary>
        ///Constructor que recibe el contexto de la base de datos
        ///</summary>
        ///<param name="context">Instancia de <see cref="ApplicationDbContext"/>para la conexion con la base de datos</param>
        public RolData(ApplicationDbContext context, ILogger<Rol> logger)
        {
            _context = context;
            _logger = logger;
        }

        ///<summary>
        ///Crea un nuevo rol en la base de datos
        ///</summary>
        ///<param name="rol">Instancia del rol a crear</param>
        ///<returns>El rol creado</returns>
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

        ///<summary>
        ///Obtiene todos los roles almacenados en la base de datos
        ///</summary>
        ///<returns>Lista de los roles</returns>
        public async Task<IEnumerable<Rol>> GetAllAsync()
        {
            IEnumerable<Rol> lstRols = await _context.Set<Rol>().ToListAsync();
            return lstRols;
        }
        
        ///<summary>Obtiene un rol especifico por su identificador</summary>
        public async Task<Rol?> GetByIdAsync(int id)
        {
            try
            {
                return await _context.Set<Rol>().FindAsync(id);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error al obtener rol con ID {RolId}  " + ex.Message, id);
                throw;
            }
        }
        ///<summary>
        ///Actualiza un rol existente en la base de datos
        ///</summary>
        ///<param name="rol">Objeto con la informacion actualizada</param>
        ///<returns>True si la operacion fue exitosa, false en caso contrario</returns>
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
        ///<summary>
        ///Elimina un rol de la base de datos
        ///</summary>
        ///<param name="id">Identificador unico del rol a eliminar</param>
        ///<returns>True si la eliminacion fue exitosa, false en caso contrario</returns>
        ///
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
                Console.WriteLine($"Error al elminar el rol: {ex.Message}");
                return false;
            }
        }


            /// <summary>
    /// Realiza una eliminación lógica del rol, marcándolo como inactivo.
    /// </summary>
    /// <param name="id">ID del rol a desactivar</param>
    /// <returns>True si se desactivó correctamente, false si no se encontró</returns>
    public async Task<bool> DisableAsync(int id)
    {
        try
        {
            var rol = await _context.Set<Rol>().FindAsync(id);
            if (rol == null)
                return false;

            rol.Active = false;
            _context.Set<Rol>().Update(rol);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al realizar eliminación lógica del rol con ID {rolId}", id);
            return false;
        }
    }

    //datos de patch para actualizar parcialmente un formulario
    public async Task PartialUpdateFormAsync(Rol rol, params string[] propertiesToUpdate)
{
    var entry = _context.Entry(rol);

    foreach (var property in propertiesToUpdate)
    {
        entry.Property(property).IsModified = true;
    }

    await _context.SaveChangesAsync();
}

    }
}
