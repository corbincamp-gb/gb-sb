using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Expressions;
using CsvHelper.TypeConversion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using SkillBridge_System_Prototype.Data;
using SkillBridge_System_Prototype.Models;
using SkillBridge_System_Prototype.Util.Global;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Z.EntityFramework.Plus;
using SkillBridge_System_Prototype.Enums;

namespace SkillBridge_System_Prototype.Util.Global
{
    public static class GlobalFunctions
    {
        public readonly static int MIN_STATES_FOR_NATIONWIDE = 3;

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

        public static int FindNumStatesInProgram(SB_Program prog, ApplicationDbContext _db)
        {
            // Update Program
            string newStateList = "";
            int num = 0;

            var relatedOpps = _db.Opportunities.Where(p => p.Program_Id == prog.Id);

            List<string> states = new List<string>();

            // Make sure there aren't duplicate states in list
            foreach (SB_Opportunity o in relatedOpps)
            {
                bool found = false;

                foreach (string s in states)
                {
                    if (s == o.State)
                    {
                        found = true;
                        continue;
                    }
                }

                if (found == false)
                {
                    if (o.State != "" && o.State != " ")
                    {
                        states.Add(o.State);
                    }
                }
            }

            // Sort states alphabetically
            states.Sort();

            // Format states in string
            foreach (string s in states)
            {
                if (num == 0)
                {
                    newStateList += s;
                }
                else
                {
                    newStateList += ", " + s;
                }
                num++;
            }

            //prog.States_Of_Program_Delivery = newStateList;
            //_db.SaveChanges();

            return num;
        }

        public static void UpdateOrgStatesOfProgramDelivery(SB_Organization org, ApplicationDbContext _db)
        {
            // Get all programs from org
            List<SB_Program> progs = _db.Programs.Where(e => e.Organization_Id == org.Id).ToList();

            List<string> states = new List<string>();


            foreach (SB_Program p in progs)
            {
                string progStates = "";
                progStates = p.States_Of_Program_Delivery;
                Console.WriteLine("progStates: " + progStates);

                // Split out each programs states of program delivery, and add them to the states array
                progStates = progStates.Replace(" ", "");
                string[] splitStates = progStates.Split(",");

                foreach (string s in splitStates)
                {
                    if (s != "" && s != " ")
                    {
                        Console.WriteLine("s in splitstates: " + s);
                        bool found = false;

                        foreach (string st in states)
                        {
                            if (s == st)
                            {
                                found = true;
                                break;
                            }
                        }

                        if (found == false)
                        {
                            states.Add(s);
                        }
                    }
                }
            }

            // Sort states alphabetically
            states.Sort();

            // Go through and remove duplicate entries
            int count = 0;
            string orgStates = "";

            foreach (string s in states)
            {
                Console.WriteLine("Checking state s: " + s);

                if (count == 0)
                {
                    orgStates += s;
                }
                else
                {
                    orgStates += ", " + s;
                }

                count++;
            }

            org.States_Of_Program_Delivery = orgStates;

            _db.SaveChanges();
        }

        /* TODO: The program availability is editable in the program level upon creation, in the future if there are no opportunities, count should be based off of initial states_of_program_delivery values */
        public static void UpdateStatesOfProgramDelivery(SB_Program prog, List<SB_Opportunity> opps, ApplicationDbContext _db)
        {
            // Update Program
            string newStateList = "";
            int num = 0;
            int activeOppsCount = 0;
            int individualActiveOppStates = 0;
            bool locationsAvailable = false;

            List<string> states = new List<string>();

            // Make sure there aren't duplicate states in list
            foreach (SB_Opportunity o in opps)
            {
                if (o.Is_Active == true)
                {
                    locationsAvailable = true;
                    bool found = false;

                    foreach (string s in states)
                    {
                        if (s == o.State)
                        {
                            found = true;
                            continue;
                        }
                    }

                    if (found == false)
                    {
                        if (o.State != "" && o.State != " ")
                        {
                            states.Add(o.State);
                            individualActiveOppStates++;
                        }
                    }
                
                    activeOppsCount++;
                }
            }

            // Sort states alphabetically
            states.Sort();

            // Format states in string
            foreach (string s in states)
            {
                if (num == 0)
                {
                    newStateList += s;
                }
                else
                {
                    newStateList += ", " + s;
                }
                num++;
            }

            prog.States_Of_Program_Delivery = newStateList;

            // If more than one active opportunity, or if more than 2 states overall in opps, prog has multiple locations
            if (activeOppsCount > 1 || num > 1)
            {
                prog.Has_Multiple_Locations = true;
            }
            else
            {
                prog.Has_Multiple_Locations = false;
            }

            if(individualActiveOppStates >= MIN_STATES_FOR_NATIONWIDE || prog.Online)
            {
                prog.Nationwide = true;
            }
            else
            {
                prog.Nationwide = false;
            }

            prog.Location_Details_Available = locationsAvailable;

            _db.SaveChanges();
        }

        public static void UpdateProgramLocationDetailsAvailable(SB_Program prog, ApplicationDbContext _db)
        {
            List<SB_Opportunity> opps = _db.Opportunities.Where(m => m.Program_Id == prog.Id).ToList();

            bool locationsAvailable = false;

            foreach(SB_Opportunity o in opps)
            {
                if(o.Is_Active)
                {
                    locationsAvailable = true;  // Once we find one active opp, we can stop looking
                    break;
                }
            }

            prog.Location_Details_Available = locationsAvailable;

            _db.SaveChanges();
        }

        /* TODO: Make this update the Opportunities Job Families optimized field underneath a Program*/
        public static void UpdateJobFamilyListForOpps(SB_Program prog, List<SB_Opportunity> opps, ApplicationDbContext _db)
        {

        }

        public static string GetDeliveryMethod(int Delivery_Method)
        {
            switch (Delivery_Method)
            {
                case 0:
                case 1:
                    return "In-Person";
                case 2:
                    return "Online";
                case 3:
                    return "Hybrid (In-Person and Online)";
                default:
                    return "N/A";
            }
        }

        public static string GetProgramDuration(int Program_Duration)
        {
            switch (Program_Duration)
            {
                case 0:
                    return "1 - 30 days";
                case 1:
                    return "31 - 60 days";
                case 2:
                    return "61 - 90 days";
                case 3:
                    return "91 - 120 days";
                case 4:
                    return "121 - 150 days";
                case 5:
                    return "151 - 180 days";
                case 6:
                    return "Individually Developed – not to exceed 40 hours";
                case 7:
                    return "Self - paced";
                default:
                    return "N/A";
            }
        }

        public static string GetEntryStatus(int status)
        {
            switch (status)
            {
                case (int)IntakeForm.Models.Enumerations.EntryStatus.Started:
                    return "Started by Applicant";
                case (int)IntakeForm.Models.Enumerations.EntryStatus.Submitted:
                    return "Submitted by Applicant";
                case (int)IntakeForm.Models.Enumerations.EntryStatus.Incomplete:
                    return "Incomplete";
                case (int)IntakeForm.Models.Enumerations.EntryStatus.UnderReview:
                    return "Pending Review";
                case (int)IntakeForm.Models.Enumerations.EntryStatus.PendingDetermination:
                    return "Pending Determination";
                case (int)IntakeForm.Models.Enumerations.EntryStatus.YesIf:
                    return "Yes - If, More Info Needed";
                case (int)IntakeForm.Models.Enumerations.EntryStatus.Approved:
                    return "Approved";
                case (int)IntakeForm.Models.Enumerations.EntryStatus.Rejected:
                    return "Not Considered";
                default:
                    return "[Unknown]";
            }
        }
    }
}
