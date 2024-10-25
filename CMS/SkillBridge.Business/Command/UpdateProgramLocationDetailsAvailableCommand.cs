using SkillBridge.Business.Model.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkillBridge.Business.Data;

namespace SkillBridge.Business.Command
{
    internal class UpdateProgramLocationDetailsAvailableCommand
    {
        public void Execute(ProgramModel prog, ApplicationDbContext _db)
        {
            var opps = _db.Opportunities.Where(m => m.Program_Id == prog.Id).ToList();

            bool locationsAvailable = false;

            foreach (var o in opps)
            {
                if (o.Is_Active)
                {
                    locationsAvailable = true;  // Once we find one active opp, we can stop looking
                    break;
                }
            }

            prog.LocationDetailsAvailable = locationsAvailable;

            _db.SaveChanges();
        }
    }
}
