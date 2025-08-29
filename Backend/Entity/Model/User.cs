using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.Model
{
    public class User
    {
    public int Id { get; set; }
    public int PersonId { get; set; }
    public Person Person { get; set; }
    public string UserName { get; set; }
  public string PasswordHash { get; set; }
    public string Code { get; set; }
    public bool Active { get; set; }
    public DateTime CreateAt { get; set; }
    public DateTime? DeleteAt { get; set; }

    }
}
