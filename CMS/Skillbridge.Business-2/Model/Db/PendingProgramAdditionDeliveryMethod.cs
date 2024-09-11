namespace Skillbridge.Business.Model.Db
{
    public class PendingProgramAdditionDeliveryMethod
    { 
        public int Id { get; set; }
        public int Pending_Program_Id { get; set; } // Id of the pending program addition
        public int Delivery_Method_Id { get; set; }    // Id of the job family
    }
}
