using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillBridgeConsoleApp.Data
{
    [Table("Organizations")]
    public class Organization
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
