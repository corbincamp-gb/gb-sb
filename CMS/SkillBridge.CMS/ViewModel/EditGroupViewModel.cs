using System.ComponentModel.DataAnnotations;

namespace SkillBridge.CMS.ViewModel
{
    public class EditGroupViewModel
    {
        public int Group_Id { get; set; }   // This is the number that will appear in the data on the site as groupid
        [Display(Name = "Latitude")]
        [Required]
        public double Lat { get; set; }
        [Display(Name = "Longitude")]
        [Required]
        public double Long { get; set; }
        [Display(Name = "Address")]
        public string Address { get; set; }
        [Display(Name = "Title")]
        public string Title { get; set; }
    }
}