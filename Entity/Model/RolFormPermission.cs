using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.Model
{
    class RolFormPermission
    {
        public int Id { get; set; }
        public Rol RolId { get; set; }
        public Permission PermissionId { get; set; }
        public Form FormId { get; set; }

        public RolFormPermission(int id, Rol rolId, Permission permissionId, Form formId)
        {
            Id = id;
            RolId = rolId;
            PermissionId = permissionId;
            FormId = formId;
        }
    }
}
