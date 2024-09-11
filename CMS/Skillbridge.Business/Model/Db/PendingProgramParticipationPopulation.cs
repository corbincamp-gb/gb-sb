namespace SkillBridge.Business.Model.Db
{
    public class PendingProgramParticipationPopulation
    {
        public int Id { get; set; }
        public int Program_Id { get; set; } // Id of the original program
        public int Pending_Program_Id { get; set; } // Id of the pending program change
        public int Participation_Population_Id { get; set; }    // Id of the participation population
    }
}
