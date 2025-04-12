using Data;
using Entity.DTOs;
using Entity.Model;
using Microsoft.Extensions.Logging;
using System;
using Utilities.Exceptions;

namespace Business
{
    /// <summary>
    /// Clase de negocio encargada de la lógica relacionada con los roles del sistema.
    /// </summary>
    public class RolBusiness
    {
        private readonly RolData _rolData;
        private readonly ILogger<Rol> _logger;

        public RolBusiness(RolData rolData, ILogger<Rol> logger)
        {
            _rolData = rolData;
            _logger = logger;
        }

        // Obtener todos los roles
        public async Task<IEnumerable<RolDto>> GetAllRolesAsync()
        {
            try
            {
                var roles = await _rolData.GetAllAsync();
                return MapToDtoList(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los roles " + ex.Message);
                throw new ExternalServiceException("Base de datos", "Error al recuperar la lista de roles", ex);
            }
        }

        // Obtener rol por ID
        public async Task<RolDto> GetRolByIdAsync(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("ID inválido al buscar rol: {RolId}", id);
                throw new ValidationException("id", "El ID del rol debe ser mayor que cero");
            }

            try
            {
                var rol = await _rolData.GetByIdAsync(id);
                if (rol == null)
                {
                    _logger.LogInformation("Rol no encontrado con ID: {RolId}", id);
                    throw new EntityNotFoundException("Rol", id);
                }

                return MapToDto(rol);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener rol con ID: {RolId} " + ex.Message, id);
                throw new ExternalServiceException("Base de datos", $"Error al recuperar el rol con ID {id}", ex);
            }
        }

        // Crear nuevo rol
        public async Task<RolDto> CreateRolAsync(RolDto rolDto)
        {
            try
            {
                ValidateRol(rolDto);

                var entity = MapToEntity(rolDto);
                var createdRol = await _rolData.CreateAsync(entity);

                return MapToDto(createdRol);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear nuevo rol: {RolNombre}", rolDto?.Name ?? "null");
                throw new ExternalServiceException("Base de datos", "Error al crear el rol", ex);
            }
        }

        // Actualizar rol existente
        public async Task<RolDto> UpdateRolAsync(int id, RolDto rolDto)
        {
            if (rolDto == null || rolDto.Id <= 0)
            {
                throw new ValidationException("id", "El ID del rol debe ser mayor que cero");
            }

            try
            {
                ValidateRol(rolDto);

                var existing = await _rolData.GetByIdAsync(rolDto.Id);
                if (existing == null)
                {
                    _logger.LogInformation("Rol no encontrado para actualizar: {RolId}", rolDto.Id);
                    throw new EntityNotFoundException("Rol", rolDto.Id);
                }

                var updated = await _rolData.UpdateAsync(MapToEntity(rolDto));
                return MapToDto(updated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar rol con ID: {RolId}", rolDto.Id);
                throw new ExternalServiceException("Base de datos", "Error al actualizar el rol", ex);
            }
        }

        private RolDto MapToDto(bool updated)
        {
            throw new NotImplementedException();
        }

        // Eliminar rol
        public async Task DeleteRolAsync(int id)
        {
            if (id <= 0)
            {
                throw new ValidationException("id", "El ID del rol debe ser mayor que cero");
            }

            try
            {
                var existing = await _rolData.GetByIdAsync(id);
                if (existing == null)
                {
                    _logger.LogInformation("Rol no encontrado para eliminar con ID: {RolId}", id);
                    throw new EntityNotFoundException("Rol", id);
                }

                await _rolData.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar rol con ID: {RolId}", id);
                throw new ExternalServiceException("Base de datos", "Error al eliminar el rol", ex);
            }
        }

        // Validar DTO de rol
        private void ValidateRol(RolDto rolDto)
        {
            if (rolDto == null)
            {
                throw new ValidationException("El objeto rol no puede ser nulo");
            }

            if (string.IsNullOrWhiteSpace(rolDto.Name))
            {
                _logger.LogWarning("Nombre vacío al crear/actualizar rol");
                throw new ValidationException("Name", "El Name del rol es obligatorio");
            }
        }

        // Mapeo a DTO
        private RolDto MapToDto(Rol rol)
        {
            return new RolDto
            {
                Id = rol.Id,
                Name = rol.Name,
                Description = rol.Description,
                Active = rol.Active
            };
        }

        // Mapeo a entidad
        private Rol MapToEntity(RolDto dto)
        {
            return new Rol
            {
                Id = dto.Id,
                Name = dto.Name,
                Description = dto.Description,
                Active = dto.Active
            };
        }

        // Mapeo de lista
        private IEnumerable<RolDto> MapToDtoList(IEnumerable<Rol> roles)
        {
            return roles.Select(MapToDto).ToList();
        }
    }
}
