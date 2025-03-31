using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.Model
{
    public class Module
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime DeleteAt { get; set; }

        public Module(int id, string name, bool active, DateTime createAt, DateTime deleteAt)
        {
            Id = id;
            Name = name;
            Active = active;
            CreateAt = createAt;
            DeleteAt = deleteAt;
        }
    }
}
