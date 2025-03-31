using Entity.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.DTOs
{
    public class RolUserDTO
    {
        public int Id { get; set; }
        public UserDTO UserId { get; set; }
        public RolDto RolId { get; set; }

      
    }
}
