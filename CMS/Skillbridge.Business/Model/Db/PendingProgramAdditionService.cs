namespace Skillbridge.Business.Model.Db
{
    public class PendingProgramAdditionService
    { 
        public int Id { get; set; }
        public int Pending_Program_Id { get; set; } // Id of the pending program addition
        public int Service_Id { get; set; } // Id of the service
    }
}