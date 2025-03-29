using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.Model
{
    class RolUser
    {
        public int Id { get; set; }

        public User UserId { get; set; }
        public Rol RolId { get; set; }

        public RolUser(int id, User userId, Rol rolId)
        {
            Id = id;
            UserId = userId;
            RolId = rolId;
        }
    }
}
