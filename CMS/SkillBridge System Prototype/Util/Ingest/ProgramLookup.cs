using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkillBridge_System_Prototype.Util.Ingest
{
    public class ProgramLookup
    {
        public bool Is_Active { get; set; }
        public string Program_Name { get; set; }
        public int Program_Id { get; set; }
        public int Organization_Id { get; set; }
        public string Program_Duration { get; set; }
    }
}
