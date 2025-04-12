using Data;
using Entity.DTOs;
using Entity.Model;
using Microsoft.Extensions.Logging;
using Utilities.Exceptions;

namespace Business
{
    /// <summary>
    /// Clase de negocio encargada de la lógica relacionada con los rolUsers del sistema.
    /// </summary>
    public class RolUserBusiness
    {
        private readonly RolUserData _rolUserData;
        private readonly ILogger<RolUserBusiness> _logger;

        public RolUserBusiness(RolUserData rolUserData, ILogger<RolUserBusiness> logger)
        {
            _rolUserData = rolUserData;
            _logger = logger;
        }

        public async Task<IEnumerable<RolUserDto>> GetAllRolUsersAsync()
        {
            try
            {
                var rolUsers = await _rolUserData.GetAllAsync();
                return MapToDtoList(rolUsers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los rolUsers");
                throw new ExternalServiceException("Base de datos", "Error al recuperar la lista de rolUsers", ex);
            }
        }

        public async Task<RolUserDto> GetRolUserByIdAsync(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("ID inválido: {RolUserId}", id);
                throw new ValidationException("id", "El ID del rolUser debe ser mayor que cero");
            }

            try
            {
                var rolUser = await _rolUserData.GetByIdAsync(id);
                if (rolUser == null)
                {
                    _logger.LogInformation("No se encontró ningún rolUser con ID: {RolUserId}", id);
                    throw new EntityNotFoundException("RolUser", id);
                }

                return MapToDto(rolUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el rolUser con ID: {RolUserId}", id);
                throw new ExternalServiceException("Base de datos", $"Error al recuperar el rolUser con ID {id}", ex);
            }
        }

        public async Task<RolUserDto> CreateRolUserAsync(RolUserDto dto)
        {
            try
            {
                ValidateRolUser(dto);

                var entity = MapToEntity(dto);
                var created = await _rolUserData.CreateAsync(entity);

                return MapToDto(created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear nuevo rolUser con UserId: {UserId}, RolId: {RolId}", dto?.UserId, dto?.RolId);
                throw new ExternalServiceException("Base de datos", "Error al crear el rolUser", ex);
            }
        }

        public async Task<RolUserDto> UpdateRolUserAsync(int id, RolUserDto dto)
        {
            if (dto == null || dto.Id <= 0)
                throw new ValidationException("id", "El ID del rolUser debe ser mayor que cero");

            try
            {
                ValidateRolUser(dto);

                var existing = await _rolUserData.GetByIdAsync(dto.Id);
                if (existing == null)
                {
                    _logger.LogInformation("RolUser no encontrado para actualizar con ID: {Id}", dto.Id);
                    throw new EntityNotFoundException("RolUser", dto.Id);
                }

                var updated = await _rolUserData.UpdateAsync(MapToEntity(dto));
                return MapToDto(updated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar rolUser con ID: {Id}", dto.Id);
                throw new ExternalServiceException("Base de datos", "Error al actualizar el rolUser", ex);
            }
        }

        private RolUserDto MapToDto(bool updated)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteRolUserAsync(int id)
        {
            if (id <= 0)
                throw new ValidationException("id", "El ID del rolUser debe ser mayor que cero");

            try
            {
                var existing = await _rolUserData.GetByIdAsync(id);
                if (existing == null)
                {
                    _logger.LogInformation("RolUser no encontrado para eliminar con ID: {Id}", id);
                    throw new EntityNotFoundException("RolUser", id);
                }

                await _rolUserData.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar rolUser con ID: {Id}", id);
                throw new ExternalServiceException("Base de datos", "Error al eliminar el rolUser", ex);
            }
        }

        private void ValidateRolUser(RolUserDto dto)
        {
            if (dto == null)
                throw new ValidationException("El objeto rolUser no puede ser nulo");

            if (dto.RolId <= 0)
            {
                _logger.LogWarning("RolId inválido al crear/actualizar rolUser");
                throw new ValidationException("RolId", "El RolId del rolUser es obligatorio y debe ser mayor que cero");
            }

            if (dto.UserId <= 0)
            {
                _logger.LogWarning("UserId inválido al crear/actualizar rolUser");
                throw new ValidationException("UserId", "El UserId del rolUser es obligatorio y debe ser mayor que cero");
            }
        }

        private RolUserDto MapToDto(RolUser entity)
        {
            return new RolUserDto
            {
                Id = entity.Id,
                UserId = entity.UserId,
                RolId = entity.RolId
            };
        }

        private RolUser MapToEntity(RolUserDto dto)
        {
            return new RolUser
            {
                Id = dto.Id,
                UserId = dto.UserId,
                RolId = dto.RolId
            };
        }

        private IEnumerable<RolUserDto> MapToDtoList(IEnumerable<RolUser> entities)
        {
            return entities.Select(MapToDto).ToList();
        }

        public void CreateRol(RolUserDto rolUserDto)
        {
            throw new NotImplementedException();
        }
    }
}
