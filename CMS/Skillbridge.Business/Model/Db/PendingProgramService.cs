namespace Skillbridge.Business.Model.Db
{
    public class PendingProgramService
    { 
        public int Id { get; set; }
        public int Program_Id { get; set; } // Id of the original program
        public int Pending_Program_Id { get; set; } // Id of the pending program change
        public int Service_Id { get; set; } // Id of the service
    }
}