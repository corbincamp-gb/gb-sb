using Newtonsoft.Json;

namespace SkillBridge.Business.Model
{
    public interface IRelatedOrganizationCollection
    {
        IEnumerable<IRelatedOrganization> Data { get; set; }
    }

    public class RelatedOrganizationCollectionModel : IRelatedOrganizationCollection
    {
        [JsonProperty("data")]
        public IEnumerable<IRelatedOrganization> Data { get; set; }
    }
}
