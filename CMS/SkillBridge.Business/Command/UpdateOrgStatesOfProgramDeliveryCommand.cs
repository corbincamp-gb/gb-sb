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
            var progs = _db.Programs.Where(e => e.OrganizationId == org.Id).ToList();

            var states = new List<string>();


            foreach (var p in progs)
            {
                var progStates = "";
                progStates = p.StatesOfProgramDelivery;
                Console.WriteLine("progStates: " + progStates);

                // Split out each programs states of program delivery, and add them to the states array
                progStates = progStates.Replace(" ", "");
                var splitStates = progStates.Split(",");

                foreach (var s in splitStates)
                {
                    if (s == "" || s == " ") continue;
                    Console.WriteLine("s in splitstates: " + s);
                    var found = states.Any(st => s == st);

                    if (found == false)
                    {
                        states.Add(s);
                    }
                }
            }

            // Sort states alphabetically
            states.Sort();

            // Go through and remove duplicate entries
            var count = 0;
            var orgStates = "";

            foreach (var s in states)
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
