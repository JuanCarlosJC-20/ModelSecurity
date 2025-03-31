using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.Model
{
    public class Form
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public bool Active { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime DeleteAt { get; set; }

        public Form(int id, string name, string code, bool active, DateTime createAt, DateTime deleteAt)
        {
            Id = id;
            Name = name;
            Code = code;
            Active = active;
            CreateAt = createAt;
            DeleteAt = deleteAt;
        }
    }
}
