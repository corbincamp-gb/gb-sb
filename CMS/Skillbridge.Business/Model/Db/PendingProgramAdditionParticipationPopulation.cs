namespace Skillbridge.Business.Model.Db
{
    public class PendingProgramAdditionParticipationPopulation
    {
        public int Id { get; set; }
        public int Pending_Program_Id { get; set; } // Id of the pending program addition
        public int Participation_Population_Id { get; set; }    // Id of the participation population
    }
}
