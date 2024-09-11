using Skillbridge.Business.Model.Db;
using Skillbridge.Business.Data;

namespace Skillbridge.Business.Query
{
    public class NumberOfStatesInProgramQuery
    {
        public int Get(ProgramModel prog, ApplicationDbContext _db)
        {
            // Update Program
            string newStateList = "";
            int num = 0;

            var relatedOpps = _db.Opportunities.Where(p => p.Program_Id == prog.Id);

            List<string> states = new List<string>();

            // Make sure there aren't duplicate states in list
            foreach (var o in relatedOpps)
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
    }
}
