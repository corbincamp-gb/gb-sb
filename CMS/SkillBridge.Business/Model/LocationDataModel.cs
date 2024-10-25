using System.Text.Json.Serialization;

namespace SkillBridge.Business.Model
{
    public interface ILocationData
    {
        IEnumerable<ILocationItem> Locations { get; set; }
    }

    public  class LocationDataModel : ILocationData
    {
        public IEnumerable<ILocationItem> Locations { get; set; }
    }
}
