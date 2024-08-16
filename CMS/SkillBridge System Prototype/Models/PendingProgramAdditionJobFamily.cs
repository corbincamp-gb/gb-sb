using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkillBridge_System_Prototype.Models
{
    public class PendingProgramAdditionJobFamily
    {
        public int Id { get; set; }
        public int Pending_Program_Id { get; set; } // Id of the pending program addition
        public int Job_Family_Id { get; set; }    // Id of the job family
    }
}
