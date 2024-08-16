using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillBridgeConsoleApp.Data
{
    [Table("Mous")]
    public class Mou
    {
        [Key]
        public int Id { get; set; }
        public string Url { get; set; }
    }
}
