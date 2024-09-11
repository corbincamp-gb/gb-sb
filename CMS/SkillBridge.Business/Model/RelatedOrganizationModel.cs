using Newtonsoft.Json;

namespace SkillBridge.Business.Model
{
    public interface IRelatedOrganization
    {
        string Parent { get; set; }
        IEnumerable<string> Organizations { get; set; }
    }

    public class RelatedOrganizationModel : IRelatedOrganization
    {
        [JsonProperty("parentOrg")]
        public string Parent { get; set; }
        [JsonProperty("orgs")]
        public IEnumerable<string> Organizations { get; set; }

    }
}
