using System.ComponentModel.DataAnnotations;

namespace Taku.Core.Enums
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