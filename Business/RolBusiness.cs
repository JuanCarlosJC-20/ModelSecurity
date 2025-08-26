using Data;
using Entity.Context;
using Entity.DTOs;
using Entity.Model;
using Microsoft.Extensions.Logging;
using Utilities.Exceptions;

namespace Business
{
    public class RolBusiness
    {
        private readonly RolData<SqlServerDbContext> _sqlRolData;
        private readonly RolData<PostgresDbContext> _pgRolData;
        private readonly RolData<MySqlDbContext> _myRolData;
        private readonly ILogger<RolBusiness> _logger;

        public RolBusiness(
            RolData<SqlServerDbContext> sqlRolData,
            RolData<PostgresDbContext> pgRolData,
            RolData<MySqlDbContext> myRolData,
            ILogger<RolBusiness> logger)
        {
            _sqlRolData = sqlRolData;
            _pgRolData = pgRolData;
            _myRolData = myRolData;
            _logger = logger;
        }

        public async Task<IEnumerable<RolDto>> GetAllRolesAsync()
        {
            try
            {
                var roles = await _sqlRolData.GetAllAsync();
                return MapToDtoList(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los roles");
                throw new ExternalServiceException("Base de datos", "Error al recuperar la lista de roles", ex);
            }
        }

        public async Task<RolDto> GetRolByIdAsync(int id)
        {
            if (id <= 0)
                throw new ValidationException("id", "El ID del rol debe ser mayor que cero");

            var rol = await _sqlRolData.GetByIdAsync(id);
            if (rol == null)
                throw new EntityNotFoundException("Rol", id);

            return MapToDto(rol);
        }

        public async Task<RolDto> CreateRolAsync(RolDto rolDto)
        {
            try
            {
                ValidateRol(rolDto);
                var rol = MapToEntity(rolDto);

                // Crear en todas las bases de datos
                var sqlRol = await _sqlRolData.CreateAsync(rol);
                await _pgRolData.CreateAsync(rol);
                await _myRolData.CreateAsync(rol);

                return MapToDto(sqlRol);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear rol: {RolName}", rolDto?.Name);
                throw;
            }
        }

        public async Task<RolDto> UpdateRolAsync(int id, RolDto rolDto)
        {
            if (id <= 0 || rolDto == null)
                throw new ValidationException("id", "El ID del rol debe ser mayor que cero");

            ValidateRol(rolDto);

            try
            {
                var rol = MapToEntity(rolDto);

                // Actualizar en todas las bases de datos
                await _sqlRolData.UpdateAsync(rol);
                await _pgRolData.UpdateAsync(rol);
                await _myRolData.UpdateAsync(rol);

                return MapToDto(rol);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar rol con ID: {RolId}", id);
                throw;
            }
        }

        public async Task DeleteRolAsync(int id)
        {
            if (id <= 0)
                throw new ValidationException("id", "El ID del rol debe ser mayor que cero");

            try
            {
                await _sqlRolData.DeleteAsync(id);
                await _pgRolData.DeleteAsync(id);
                await _myRolData.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar rol con ID: {RolId}", id);
                throw;
            }
        }

        public async Task DisableRolAsync(int id)
        {
            if (id <= 0)
                throw new ValidationException("id", "El ID del rol debe ser mayor que cero");

            try
            {
                await _sqlRolData.DisableAsync(id);
                await _pgRolData.DisableAsync(id);
                await _myRolData.DisableAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al desactivar rol con ID: {RolId}", id);
                throw;
            }
        }

        public async Task PartialUpdateRolAsync(RolDto rolDto)
        {
            try
            {
                var existingRol = await _sqlRolData.GetByIdAsync(rolDto.Id);
                if (existingRol == null)
                    throw new EntityNotFoundException("Rol", rolDto.Id);

                if (!string.IsNullOrEmpty(rolDto.Name))
                    existingRol.Name = rolDto.Name;
                if (!string.IsNullOrEmpty(rolDto.Description))
                    existingRol.Description = rolDto.Description;

                existingRol.Active = rolDto.Active;

                // Actualizar parcialmente en todas las bases de datos
                await _sqlRolData.PartialUpdateRolAsync(existingRol,
                    nameof(existingRol.Name),
                    nameof(existingRol.Description),
                    nameof(existingRol.Active));

                await _pgRolData.PartialUpdateRolAsync(existingRol,
                    nameof(existingRol.Name),
                    nameof(existingRol.Description),
                    nameof(existingRol.Active));

                await _myRolData.PartialUpdateRolAsync(existingRol,
                    nameof(existingRol.Name),
                    nameof(existingRol.Description),
                    nameof(existingRol.Active));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar parcialmente el rol con ID: {RolId}", rolDto.Id);
                throw;
            }
        }

        private void ValidateRol(RolDto rolDto)
        {
            if (rolDto == null)
                throw new ValidationException("El objeto rol no puede ser nulo");

            if (string.IsNullOrWhiteSpace(rolDto.Name))
                throw new ValidationException("Name", "El nombre del rol es obligatorio");
        }

        private RolDto MapToDto(Rol rol) => new()
        {
            Id = rol.Id,
            Name = rol.Name,
            Description = rol.Description,
            Active = rol.Active
        };

        private Rol MapToEntity(RolDto dto) => new()
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            Active = dto.Active
        };

        private IEnumerable<RolDto> MapToDtoList(IEnumerable<Rol> roles)
            => roles.Select(MapToDto);
    }
}
