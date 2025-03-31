using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.Model
{
    public class FormModule
    {
        public int Id { get; set; }
        public Module ModuleId { get; set; }
        public Form FormId { get; set; }

        public FormModule(int id, Module moduleId, Form formId)
        {
            Id = id;
            ModuleId = moduleId;
            FormId = formId;
        }
    }
}
