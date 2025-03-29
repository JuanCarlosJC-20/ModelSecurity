using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.Model
{
    class User
    {
        public int Id { get; set; }
        public Person PersonId { get; set; }
        public string UserName { get; set; }
        public string Code { get; set; }
        public bool Active { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime DeletAt { get; set; }

        public User(int id, Person personId, string userName, string code, bool active, DateTime createAt, DateTime deletAt)
        {
            Id = id;
            PersonId = personId;
            UserName = userName;
            Code = code;
            Active = active;
            CreateAt = createAt;
            DeletAt = deletAt;
        }
    }
}
