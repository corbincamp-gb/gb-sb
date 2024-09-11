using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntakeForm.Models.View.Admin
{
    public class AdminSearchModel
    {
        public int? EntryStatusId { get; set; }
        public string ZohoTicketId { get; set; }
        public string Organization { get; set; }
        public DateTime? SubmittedStartingOn { get; set; }
        public DateTime? SubmittedEndingOn { get; set; }
        public DateTime? LastUpdatedStartingOn { get; set; }
        public DateTime? LastUpdatedEndingOn { get; set; }

        public string SortBy { get; set; }
        public string SortOrder { get; set; }
    }
}
