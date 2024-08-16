using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkillBridge_System_Prototype.Models
{
    public class PendingProgramAdditionParticipationPopulation
    {
        public int Id { get; set; }
        public int Pending_Program_Id { get; set; } // Id of the pending program addition
        public int Participation_Population_Id { get; set; }    // Id of the participation population
    }
}
