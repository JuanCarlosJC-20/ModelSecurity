using Data;
using Entity.DTOs;
using Entity.Model;
using Microsoft.Extensions.Logging;
using Microsoft.SqlServer.Server;
using Utilities.Exceptions;

namespace Business
{
    /// <summary>
    /// Clase de negocio encargada de la lógica relacionada con los RolFormPermission del sistema.
    /// </summary>
    public class RolFormPermissionBusiness
    {
        private readonly RolFormPermissionData _rolFormPermissionData;
        private readonly ILogger _logger;

        public RolFormPermissionBusiness(RolFormPermissionData rolFormPermissionData, ILogger logger)
        {
            _rolFormPermissionData = rolFormPermissionData;
            _logger = logger;
        }

        // Método para obtener todos los RolFormPermissio como DTOs
        public async Task<IEnumerable<RolFormPermissionDto>> GetAllRolFormPermissionsAsync()
        {
            try
            {
                var rolFormPermissions = await _rolFormPermissionData.GetAllAsync();
                var rolFormPermissionsDto = new List<RolFormPermissionDto>();

                foreach (var rolFormPermission in rolFormPermissions)
                {
                    rolFormPermissionsDto.Add(new RolFormPermissionDto
                    {
                        Id = rolFormPermission.Id,
                        RolId = rolFormPermission.RolId,
                        PermissionId = rolFormPermission.PermissionId,
                        FormId = rolFormPermission.FormId
                    });
                }

                return rolFormPermissionsDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los rolFormPermissions");
                throw new ExternalServiceException("Base de datos", "Error al recuperar la lista de rolFormPermissions", ex);
            }
        }

        // Método para obtener un rolFormPermission por ID como DTO
        public async Task<RolFormPermissionDto> GetRolFormPermissionByIdAsync(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Se intentó obtener un rolFormPermission con ID inválido: {RolFormPermissionId}", id);
                throw new Utilities.Exceptions.ValidationException("id", "El ID del rolFormPermission debe ser mayor que cero");
            }

            try
            {
                var rolFormPermission = await _rolFormPermissionData.GetByIdAsync(id);
                if (rolFormPermission == null)
                {
                    _logger.LogInformation("No se encontró ningún rolFormPermission con ID: {RolFormPermissionId}", id);
                    throw new EntityNotFoundException("RolFormPermissiion", id);
                }

                return new RolFormPermissionDto
                {
                    Id = rolFormPermission.Id,
                    RolId = rolFormPermission.RolId,
                    PermissionId = rolFormPermission.PermissionId,
                    FormId = rolFormPermission.FormId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el rolFormPermission con ID: {RolFormPermissionId}", id);
                throw new ExternalServiceException("Base de datos", $"Error al recuperar el rolFormPermission con ID {id}", ex);
            }
        }

        // Método para crear un rolFormPermission desde un DTO
        public async Task<RolFormPermissionDto> CreateRolFormPermissionAsync(RolFormPermissionDto rolFormPermissionDto)
        {
            try
            {
                ValidateRolFormPermission(rolFormPermissionDto);

                var rol = new RolFormPermission
                {
                    RolId = rolFormPermissionDto.RolId,
                    PermissionId = rolFormPermissionDto.PermissionId,
                    FormId = rolFormPermissionDto.FormId
                };

                var rolFormPermissionCreado = await _rolFormPermissionData.CreateAsync(rol);

                return new RolFormPermissionDto
                {
                    Id = rolFormPermissionCreado.Id,
                    RolId = rolFormPermissionCreado.RolId,
                    PermissionId = rolFormPermissionCreado.PermissionId,
                    FormId = rolFormPermissionCreado.FormId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear nuevo formModule: {FormModuleRolId}", rolFormPermissionDto.RolId <= 0);
                _logger.LogError(ex, "Error al crear nuevo formModule: {FormModulePermissionId}", rolFormPermissionDto.PermissionId <= 0);
                _logger.LogError(ex, "Error al crear nuevo formModule: {FormModuleFormId}", rolFormPermissionDto.FormId <= 0);
                throw new ExternalServiceException("Base de datos", "Error al crear el rolFormPermission", ex);
            }
        }

        // Método para validar el DTO
        private void ValidateRolFormPermission(RolFormPermissionDto rolFormPermissionDto)
        {
            if (rolFormPermissionDto == null)
            {
                throw new Utilities.Exceptions.ValidationException("El objeto rolFormPermission no puede ser nulo");
            }

            if (rolFormPermissionDto.RolId <= 0)
            {
                _logger.LogWarning("Se intentó crear/actualizar un rolFormPermisison con RolId vacío");
                throw new Utilities.Exceptions.ValidationException("RolId", "El Name del rolFormPermission es obligatorio");
            }
            if (rolFormPermissionDto.PermissionId <= 0)
            {
                _logger.LogWarning("Se intentó crear/actualizar un rolFormPermisison con PermissionId vacío");
                throw new Utilities.Exceptions.ValidationException("PermissionId", "El Name del rolFormPermission es obligatorio");
            }
            if (rolFormPermissionDto.FormId <= 0)
            {
                _logger.LogWarning("Se intentó crear/actualizar un rolFormPermisison con FormId vacío");
                throw new Utilities.Exceptions.ValidationException("FormId", "El Name del rolFormPermission es obligatorio");
            }
        }

        public async Task GetAllRolFormPermissionAsync()
        {
            throw new NotImplementedException();
        }
    }
}