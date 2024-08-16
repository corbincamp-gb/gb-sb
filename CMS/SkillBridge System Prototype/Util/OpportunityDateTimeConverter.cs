using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkillBridge_System_Prototype.Util
{
    public class OpportunityDateTimeConverter : IsoDateTimeConverter
    {
        public OpportunityDateTimeConverter()
        {
            base.DateTimeFormat = "MM/dd/yyyy";
        }
    }
}



