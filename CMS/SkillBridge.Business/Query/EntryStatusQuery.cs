using Taku.Core;
using static IntakeForm.Models.Enumerations;

namespace SkillBridge.Business.Query
{
    public interface IEntryStatusQuery : IQuery
    {
        string Get(int status);
    }

    public class EntryStatusQuery : IEntryStatusQuery
    {
        public string Get(EntryStatus status)
        {
            return Get(status.GetHashCode());
        }

        public string Get(int status)
        {
            var stats = new Dictionary<int, string>
            {
                { EntryStatus.Started.GetHashCode(), "Started by Applicant" },
                { EntryStatus.Submitted.GetHashCode(), "Submitted by Applicant" },
                { EntryStatus.Incomplete.GetHashCode(), "Incomplete" },
                { EntryStatus.UnderReview.GetHashCode(), "Pending Review" },
                { EntryStatus.PendingDetermination.GetHashCode(), "Pending Determination" },
                { EntryStatus.YesIf.GetHashCode(), "Yes - If, More Info Needed" },
                { EntryStatus.Approved.GetHashCode(), "Approved" },
                { EntryStatus.Rejected.GetHashCode(), "Not Considered" },
            };

            stats.TryGetValue(status, out var ret);

            return ret ?? "[unknown]";


            //switch (status)
            //{
            //    case (int)IntakeForm.Models.Enumerations.EntryStatus.Started:
            //        return "Started by Applicant";
            //    case (int)IntakeForm.Models.Enumerations.EntryStatus.Submitted:
            //        return "Submitted by Applicant";
            //    case (int)IntakeForm.Models.Enumerations.EntryStatus.Incomplete:
            //        return "Incomplete";
            //    case (int)IntakeForm.Models.Enumerations.EntryStatus.UnderReview:
            //        return "Pending Review";
            //    case (int)IntakeForm.Models.Enumerations.EntryStatus.PendingDetermination:
            //        return "Pending Determination";
            //    case (int)IntakeForm.Models.Enumerations.EntryStatus.YesIf:
            //        return "Yes - If, More Info Needed";
            //    case (int)IntakeForm.Models.Enumerations.EntryStatus.Approved:
            //        return "Approved";
            //    case (int)IntakeForm.Models.Enumerations.EntryStatus.Rejected:
            //        return "Not Considered";
            //    default:
            //        return "[Unknown]";
            //}
        }
    }
}
