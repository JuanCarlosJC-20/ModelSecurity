using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.DTOs
{
    class RolDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime DeleteAt { get; set; }

        public RolDTO(int id, string name, bool active, DateTime createAt, DateTime deleteAt)
        {
            Id = id;
            Name = name;
            Active = active;
            CreateAt = createAt;
            DeleteAt = deleteAt;
        }
    }
}
