namespace Skillbridge.Business.Model.Db
{
    public class PendingProgramJobFamily
    {
        public int Id { get; set; }
        public int Program_Id { get; set; } // Id of the original program
        public int Pending_Program_Id { get; set; } // Id of the pending program change
        public int Job_Family_Id { get; set; }    // Id of the job family
    }
}
