using Data;
using Entity.DTOs;
using Entity.Model;
using Microsoft.Extensions.Logging;
using System.Security;
using Utilities.Exceptions;

namespace Business
{
    /// <summary>
    /// Clase de negocio encargada de la lógica relacionada con los permisos del sistema.
    /// </summary>
    public class PermissionBusiness
    {
        private readonly PermissionData _permissionData;
        private readonly ILogger _logger;

        public PermissionBusiness(PermissionData permissionData, ILogger logger)
        {
            _permissionData = permissionData;
            _logger = logger;
        }

        // Método para obtener todos los permisos como DTOs
        public async Task<IEnumerable<PermissionDto>> GetAllPermissionAsync()
        {
            try
            {
                var permissions = await _permissionData.GetAllAsync();
                var permissionsDTO = new List<PermissionDto>();

                foreach (var permission in permissions)
                {
                    permissionsDTO.Add(new PermissionDto
                    {
                        Id = permission.Id,
                        Name = permission.Name,
                        Code = permission.Code,
                        Active = permission.Active
                    });
                }

                return permissionsDTO;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los permisos");
                throw new ExternalServiceException("Base de datos", "Error al recuperar la lista de permisos", ex);
            }
        }

        // Método para obtener un permiso por ID como DTO
        public async Task<PermissionDto> GetPermissionByIdAsync(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Se intentó obtener un permiso con ID inválido: {PermissionId}", id);
                throw new Utilities.Exceptions.ValidationException("id", "El ID del permiso debe ser mayor que cero");
            }

            try
            {
                var permission = await _permissionData.GetByIdAsync(id);
                if (permission == null)
                {
                    _logger.LogInformation("No se encontró ningún permiso con ID: {PermissionId}", id);
                    throw new EntityNotFoundException("Permission", id);
                }

                return new PermissionDto
                {
                    Id = permission.Id,
                    Name = permission.Name,
                    Code = permission.Code,
                    Active = permission.Active
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el permiso con ID: {PermissionId}", id);
                throw new ExternalServiceException("Base de datos", $"Error al recuperar el permiso con ID {id}", ex);
            }
        }

        // Método para crear un permiso desde un DTO
        public async Task<PermissionDto> CreatePermissionAsync(PermissionDto permissionDto)
        {
            try
            {
                ValidatePermission(permissionDto);

                var permission = new Permission
                {
                    Name = permissionDto.Name,
                    Code = permissionDto.Code,
                    Active = permissionDto.Active
                };

                var permissionCreado = await _permissionData.CreateAsync(permission);

                return new PermissionDto
                {
                    Id = permissionCreado.Id,
                    Name = permissionCreado.Name,
                    Code = permissionCreado.Code,
                    Active = permissionCreado.Active // Si existe en la entidad
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear nuevo permiso: {PermissionNombre}", permissionDto?.Name ?? "null");
                throw new ExternalServiceException("Base de datos", "Error al crear el permiso", ex);
            }
        }

        // Método para validar el DTO
        private void ValidatePermission(PermissionDto permissionDto)
        {
            if (permissionDto == null)
            {
                throw new Utilities.Exceptions.ValidationException("El objeto permiso no puede ser nulo");
            }

            if (string.IsNullOrWhiteSpace(permissionDto.Name))
            {
                _logger.LogWarning("Se intentó crear/actualizar un permiso con Name vacío");
                throw new Utilities.Exceptions.ValidationException("Name", "El Name del permiso es obligatorio");
            }
        }
        //Metodo para mapear PermissionDto

        private PermissionDto MapToDto(Permission permission)
        {
            return new PermissionDto
            {
                Id = permission.Id,
                Name = permission.Name,
                Code = permission.Code,
                Active = permission.Active

            };
        }



        private Permission MapToEntity(PermissionDto PermissionDto)
        {
            return new Permission
            {
                Id = PermissionDto.Id,
                Name = PermissionDto.Name,
                Code = PermissionDto.Code,
                Active = PermissionDto.Active


            };
        }
        // Método para mapear una lista de Rol a una lista de PermissionDto
        private IEnumerable<PermissionDto> MapToDtoList(IEnumerable<Permission> permissions)
        {
            var PermissionDtos = new List<PermissionDto>();
            foreach (var permission in permissions)
            {
                PermissionDtos.Add(MapToDto(permission));
            }
            return PermissionDtos;
        }
    }
}