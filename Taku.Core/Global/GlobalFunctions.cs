using System.Text.RegularExpressions;

namespace Taku.Core.Global
{
    public static class GlobalFunctions
    {
        public static readonly int MIN_STATES_FOR_NATIONWIDE = 3;

        public static string RemoveSpecialCharacters(string str = "")
        {
            //Console.WriteLine("Cleaning string: " + str);
            if(str == "" || str == null)
            {
                return "";
            }

            str = RemoveLineBreaksFromString(str);

            //"^[a-zA-Z0-9\b .!,()$?]+$"
            return Regex.Replace(str, "[^a-zA-Z0-9\b\n .!,()/@$?:-]+$", "", RegexOptions.Compiled);
        }

        public static string EscapeCharacters(string str = "")
        {
            //Console.WriteLine("Cleaning string: " + str);
            if (str == "" || str == null)
            {
                return "";
            }

            return str.Replace("\"", "\\\"");
        }

        public static string RemoveLineBreaksFromString(string a)
        {
            string b = "";

            b = a.Replace("\n", "").Replace("\r", "");

            return b;
        }

      

      
        /* TODO: The program availability is editable in the program level upon creation, in the future if there are no opportunities, count should be based off of initial states_of_program_delivery values */
      
       

        public static string? GetDeliveryMethod(int deliveryMethod)
        {
            var dms = new Dictionary<int, string?>
            {
                { 0, "In-Person" },
                { 1, "In-Person" },
                { 2, "Online" },
                { 3, "Hybrid (In-Person and Online)" }

            };

            dms.TryGetValue(deliveryMethod, out var ret);
            
            return ret;

            
        }

        public static string GetProgramDuration(int duration)
        {
            return duration switch
            {
                0 => "1 - 30 days",
                1 => "31 - 60 days",
                2 => "61 - 90 days",
                3 => "91 - 120 days",
                4 => "121 - 150 days",
                5 => "151 - 180 days",
                6 => "Individually Developed – not to exceed 40 hours",
                7 => "Self - paced",
                _ => "N/A"
            };
        }

        //public static string GetEntryStatus(int status)
        //{
        //    switch (status)
        //    {
        //        case (int)IntakeForm.Models.Enumerations.EntryStatus.Started:
        //            return "Started by Applicant";
        //        case (int)IntakeForm.Models.Enumerations.EntryStatus.Submitted:
        //            return "Submitted by Applicant";
        //        case (int)IntakeForm.Models.Enumerations.EntryStatus.Incomplete:
        //            return "Incomplete";
        //        case (int)IntakeForm.Models.Enumerations.EntryStatus.UnderReview:
        //            return "Pending Review";
        //        case (int)IntakeForm.Models.Enumerations.EntryStatus.PendingDetermination:
        //            return "Pending Determination";
        //        case (int)IntakeForm.Models.Enumerations.EntryStatus.YesIf:
        //            return "Yes - If, More Info Needed";
        //        case (int)IntakeForm.Models.Enumerations.EntryStatus.Approved:
        //            return "Approved";
        //        case (int)IntakeForm.Models.Enumerations.EntryStatus.Rejected:
        //            return "Not Considered";
        //        default:
        //            return "[Unknown]";
        //    }
        //}
    }
}
