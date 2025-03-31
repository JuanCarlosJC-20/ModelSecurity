using Entity.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.DTOs
{
    public class RolFormPermissionDTO
    {
        public int Id { get; set; }
        public RolDto RolId { get; set; }
        public PermissionDTO PermissionId { get; set; }
        public FormDTO FormId { get; set; }

    
    }
}
