using SkillBridge.Business.Model.Db;
using SkillBridge.Business.Data;
using Taku.Core;

namespace SkillBridge.Business.Command
{
    public interface IUpdateStatesOfProgramDeliveryCommand : IRenderingCommand
    {
        void Execute(ProgramModel prog, List<OpportunityModel> opps, ApplicationDbContext _db);
    }

    public class UpdateStatesOfProgramDeliveryCommand : IUpdateStatesOfProgramDeliveryCommand
    {
        public void Execute(ProgramModel prog, List<OpportunityModel> opps, ApplicationDbContext _db)
        {
            // Update Program
            var newStateList = "";
            var num = 0;
            var activeOppsCount = 0;
            var individualActiveOppStates = 0;
            var locationsAvailable = false;

            var states = new List<string>();

            // Make sure there aren't duplicate states in list
            foreach (var o in opps.Where(o => o.Is_Active))
            {
                locationsAvailable = true;
                var found = false;

                foreach (var s in states.Where(s => s == o.State))
                {
                    found = true;
                    continue;
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

            // Sort states alphabetically
            states.Sort();

            // Format states in string
            newStateList = string.Join(", ", states);

            prog.StatesOfProgramDelivery = newStateList;

            // If more than one active opportunity, or if more than 2 states overall in opps, prog has multiple locations
            prog.HasMultipleLocations = activeOppsCount > 1;

            prog.Nationwide =
                (individualActiveOppStates >= Taku.Core.Global.GlobalFunctions.MIN_STATES_FOR_NATIONWIDE ||
                 prog.Online);

            prog.LocationDetailsAvailable = locationsAvailable;

            _db.SaveChanges();
        }
    }
}