using Skillbridge.Business.Model.Db;
using Skillbridge.Business.Data;
using Taku.Core;

namespace Skillbridge.Business.Command
{
    public interface IUpdateStatesOfProgramDeliveryCommand : IRenderingCommand
    {
        void Execute(ProgramModel prog, List<OpportunityModel> opps, ApplicationDbContext _db);
    }

    public class UpdateStatesOfProgramDeliveryCommand : IUpdateStatesOfProgramDeliveryCommand
    {
        public  void Execute(ProgramModel prog, List<OpportunityModel> opps, ApplicationDbContext _db)
        {
            // Update Program
            string newStateList = "";
            int num = 0;
            int activeOppsCount = 0;
            int individualActiveOppStates = 0;
            bool locationsAvailable = false;

            List<string> states = new List<string>();

            // Make sure there aren't duplicate states in list
            foreach (var o in opps)
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

            if (individualActiveOppStates >= Taku.Core.Global.GlobalFunctions.MIN_STATES_FOR_NATIONWIDE || prog.Online)
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

    }
}
