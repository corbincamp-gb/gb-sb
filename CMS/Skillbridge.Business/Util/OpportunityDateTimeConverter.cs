using Newtonsoft.Json.Converters;

namespace Skillbridge.Business.Util
{
    public class OpportunityDateTimeConverter : IsoDateTimeConverter
    {
        public OpportunityDateTimeConverter()
        {
            DateTimeFormat = "MM/dd/yyyy";
        }
    }
}



