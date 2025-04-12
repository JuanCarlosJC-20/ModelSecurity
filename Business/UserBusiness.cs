using Data;
using Entity.DTOs;
using Entity.Model;
using Microsoft.Extensions.Logging;
using Utilities.Exceptions;

namespace Business
{
    /// <summary>
    /// Clase de negocio encargada de la lógica relacionada con los usuarios del sistema.
    /// </summary>
    public class UserBusiness
    {
        private readonly UserData _userData;
        private readonly ILogger _logger;

        public UserBusiness(UserData userData, ILogger logger)
        {
            _userData = userData;
            _logger = logger;
        }

        // Método para obtener todos las personas como DTOs
        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            try
            {
                var users = await _userData.GetAllAsync();
                var usersDTO = new List<UserDto>();

                foreach (var user in users)
                {
                    usersDTO.Add(new UserDto
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        Code = user.Code,
                        Active = user.Active
                    });
                }

                return usersDTO;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los usuarios");
                throw new ExternalServiceException("Base de datos", "Error al recuperar la lista de usuarios", ex);
            }
        }

        // Método para obtener un usuario por ID como DTO
        public async Task<UserDto> GetUserByIdAsync(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Se intentó obtener un usuario con ID inválido: {UserId}", id);
                throw new Utilities.Exceptions.ValidationException("id", "El ID del usuario debe ser mayor que cero");
            }

            try
            {
                var user = await _userData.GetByIdAsync(id);
                if (user == null)
                {
                    _logger.LogInformation("No se encontró ningún usuario con ID: {UserId}", id);
                    throw new EntityNotFoundException("User", id);
                }

                return new UserDto
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Code = user.Code,
                    Active = user.Active
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el usuario con ID: {UserId}", id);
                throw new ExternalServiceException("Base de datos", $"Error al recuperar el usuario con ID {id}", ex);
            }
        }

        // Método para crear un usuario desde un DTO
        public async Task<UserDto> CreateUserAsync(UserDto UserDto)
        {
            try
            {
                ValidateUser(UserDto);

                var user = new User
                {
                    UserName = UserDto.UserName,
                    Code = UserDto.Code,
                    Active = UserDto.Active
                };

                var userCreado = await _userData.CreateAsync(user);

                return new UserDto
                {
                    Id = userCreado.Id,
                    UserName = userCreado.UserName,
                    Code = userCreado.Code,
                    Active = userCreado.Active
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear nuevo usuario: {UserNombre}", UserDto?.UserName ?? "null");
                throw new ExternalServiceException("Base de datos", "Error al crear el usuario", ex);
            }
        }

        // Método para validar el DTO
        private void ValidateUser(UserDto UserDto)
        {
            if (UserDto == null)
            {
                throw new Utilities.Exceptions.ValidationException("El objeto User no puede ser nulo");
            }

            if (string.IsNullOrWhiteSpace(UserDto.UserName))
            {
                _logger.LogWarning("Se intentó crear/actualizar un usuario con UserName vacío");
                throw new Utilities.Exceptions.ValidationException("UserName", "El UserName del usuario es obligatorio");
            }
        }

        // Método para validar el DTO
        private void ValidateRolUser(RolUserDto RolUserDto)
        {
            if (RolUserDto == null)
            {
                throw new Utilities.Exceptions.ValidationException("El objeto rolUser no puede ser nulo");
            }

            if (RolUserDto.RolId <= 0)
            {
                _logger.LogWarning("Se intentó crear/actualizar un rolUser con RolId vacío");
                throw new Utilities.Exceptions.ValidationException("RolId", "El RolId del rol es obligatorio");
            }
        }

        //Metodo para mapear UserDto

        private UserDto MapToDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Code = user.Code,
                Active = user.Active

            };
        }



        private User MapToEntity(UserDto UserDto)
        {
            return new User
            {
                Id = UserDto.Id,
                UserName = UserDto.UserName,
                Code = UserDto.Code,
                Active = UserDto.Active


            };
        }
        // Método para mapear una lista de Rol a una lista de UserDto
        private IEnumerable<UserDto> MapToDtoList(IEnumerable<User> users)
        {
            var UserDtos = new List<UserDto>();
            foreach (var user in users)
            {
                UserDtos.Add(MapToDto(user));
            }
            return UserDtos;
        }
    }
}