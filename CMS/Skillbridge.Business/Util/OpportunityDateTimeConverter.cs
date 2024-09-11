using Newtonsoft.Json.Converters;

namespace SkillBridge.Business.Util
{
    public class OpportunityDateTimeConverter : IsoDateTimeConverter
    {
        public OpportunityDateTimeConverter()
        {
            DateTimeFormat = "MM/dd/yyyy";
        }
    }
}



