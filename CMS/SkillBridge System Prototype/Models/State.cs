using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkillBridge_System_Prototype.Models
{
    public class State
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Label { get; set; } // For Option Labels in Dropdowns
    }
}
