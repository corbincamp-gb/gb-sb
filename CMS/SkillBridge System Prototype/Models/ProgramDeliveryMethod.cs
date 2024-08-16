using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkillBridge_System_Prototype.Models
{
    public class ProgramDeliveryMethod
    {
        public int Id { get; set; }
        public int Program_Id { get; set; }
        public int Delivery_Method_Id { get; set; }
    }
}
