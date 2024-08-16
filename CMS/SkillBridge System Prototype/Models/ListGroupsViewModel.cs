using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SkillBridge_System_Prototype.Models
{
    public class ListGroupsViewModel
    {
        public int Group_Id { get; set; }   // This is the number that will appear in the data on the site as groupid
        public string Title { get; set; }
        public double Lat { get; set; }
        public double Long { get; set; }
        public List<int> Opportunities { get; set; }
    }
}
