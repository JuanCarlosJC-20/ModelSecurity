using Entity.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.DTOs
{
    public class FormModuleDTO
    {
        public int Id { get; set; }
        public ModuleDTO ModuleId { get; set; }
        public FormDTO FormId { get; set; }

     
    }
}
