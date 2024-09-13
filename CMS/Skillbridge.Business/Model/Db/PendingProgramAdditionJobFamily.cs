namespace SkillBridge.Business.Model.Db
{
    public class PendingProgramAdditionJobFamily
    {
        public int Id { get; set; }
        public int Pending_Program_Id { get; set; } // Id of the pending program addition
        public int Job_Family_Id { get; set; }    // Id of the job family
    }
}
