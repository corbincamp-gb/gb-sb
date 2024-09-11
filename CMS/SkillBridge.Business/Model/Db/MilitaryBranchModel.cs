using System.ComponentModel.DataAnnotations.Schema;

namespace SkillBridge.Business.Model.Db
{
    public interface IMilitaryBranch
    {
        int Id { get; set; }
        string Name { get; set; }
    }

    [Table("Services")]
    public class MilitaryBranchModel : IMilitaryBranch
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
