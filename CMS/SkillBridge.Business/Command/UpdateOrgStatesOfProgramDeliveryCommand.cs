using SkillBridge.Business.Model.Db;
using SkillBridge.Business.Data;
using Taku.Core;

namespace SkillBridge.Business.Command
{
    public interface IUpdateOrgStatesOfProgramDeliveryCommand : IRenderingCommand
    {
        void Execute(OrganizationModel org, ApplicationDbContext _db);
    }

    public class UpdateOrgStatesOfProgramDeliveryCommand : IUpdateOrgStatesOfProgramDeliveryCommand
    {
        public void Execute(OrganizationModel org, ApplicationDbContext _db)
        {
            // Get all programs from org
            var progs = _db.Programs.Where(e => e.Organization_Id == org.Id).ToList();

            List<string> states = new List<string>();


            foreach (var p in progs)
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

    }
}
