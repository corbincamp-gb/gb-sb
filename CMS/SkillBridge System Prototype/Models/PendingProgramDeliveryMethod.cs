using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkillBridge_System_Prototype.Models
{
    public class PendingProgramDeliveryMethod
    { 
        public int Id { get; set; }
        public int Program_Id { get; set; } // Id of the original program
        public int Pending_Program_Id { get; set; } // Id of the pending program change
        public int Delivery_Method_Id { get; set; }    // Id of the job family
    }
}
