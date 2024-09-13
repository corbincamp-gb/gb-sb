using System;
using System.ComponentModel.DataAnnotations;
// ReSharper disable InconsistentNaming

namespace SkillBridge.CMS.ViewModel
{
    public class MOUViewModel
    {
        public int Id { get; set; }  // auto-increment
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime Creation_Date { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime Expiration_Date { get; set; }
        [DataType(DataType.Url)]
        [RegularExpression(@"(https?:\/\/(?:www\.|(?!www))[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\.[^\s]{2,}|www\.[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\.[^\s]{2,}|https?:\/\/(?:www\.|(?!www))[a-zA-Z0-9]+\.[^\s]{2,}|www\.[a-zA-Z0-9]+\.[^\s]{2,})", ErrorMessage = "Please provide a valid URL, you may need to include http:// or https://")]
        public string Url { get; set; }
        public string Service { get; set; }
        public bool Is_OSD { get; set; }    
        public string Orgs { get; set; }    // This will end up being a comma separated string of org name values for orgs that are related to this MOU
    }
}
