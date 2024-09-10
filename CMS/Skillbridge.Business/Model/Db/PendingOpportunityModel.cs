namespace Skillbridge.Business.Model.Db
{
    public class PendingOpportunityModel
    {
        public int Id { get; set; }
        public int Organization_Id { get; set; }
        public int Program_Id { get; set; }
        public bool Is_Active { get; set; }
        public string Program_Name { get; set; }
        public string Installation { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string EnrollmentDates { get; set; }
        public bool IsAddition { get; set; }

        public PendingOpportunityModel() { }

        public PendingOpportunityModel(PendingOpportunityChangeModel model)
        {
            Id = model.Id;
            Organization_Id = model.Organization_Id;
            Program_Id = model.Program_Id;
            Is_Active = model.Is_Active;
            Program_Name = model.Program_Name;
            Installation = model.Installation;
            City = model.City;
            State = model.State; 
            Zip = model.Zip;
            EnrollmentDates = model.Enrollment_Dates;
            IsAddition = false;
        }

        public PendingOpportunityModel(PendingOpportunityAdditionModel model)
        {
            Id = model.Id;
            Organization_Id = model.Organization_Id;
            Program_Id = model.Program_Id;
            Is_Active = model.Is_Active;
            Program_Name = model.Program_Name;
            Installation = model.Installation;
            City = model.City;
            State = model.State;
            Zip = model.Zip;
            EnrollmentDates = model.Enrollment_Dates;
            IsAddition = true;
        }

    }
}
