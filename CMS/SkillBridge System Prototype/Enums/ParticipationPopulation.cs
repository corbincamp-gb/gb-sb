using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SkillBridge_System_Prototype.Enums
{
    public enum ParticipationPopulation
    {
        [Display(Name = "Services Members")]
        ServicesMembers = 0,
        Veterans = 1,
        [Display(Name = "Military Spouses")]
        MilitarySpouses = 2
    }
}