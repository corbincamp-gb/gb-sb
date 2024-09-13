using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillBridge.Business.Model.Db
{
    public class OpportunityGroupModel
    {
        [Key]
        public int Id { get; set; }  // auto-increment
        public int Group_Id { get; set; }   // This is the number that will appear in the data on the site as groupid
        [ForeignKey("SB_Opportunity")]
        public int Opportunity_Id { get; set; }
        public string Title { get; set; }
        public OpportunityModel SB_Opportunity { get; set; }
        public double Lat { get; set; }
        public double Long { get; set; }
    }
}
