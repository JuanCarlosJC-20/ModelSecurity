using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.Model
{
    public class Person
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastNanem { get; set; }
        public string Email { get; set; }

        public Person(int id, string firstName, string lastNanem, string email)
        {
            Id = id;
            FirstName = firstName;
            LastNanem = lastNanem;
            Email = email;
        }
    }
}
