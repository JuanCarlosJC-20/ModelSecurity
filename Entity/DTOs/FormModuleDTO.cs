using Entity.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.DTOs
{
    public class FormModuleDto
    {
        public int Id { get; set; }
        public int ModuleId { get; set; }
        public int FormId { get; set; }

    }
}
