namespace Skillbridge.Business.Model.Db
{
    public class State
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Label { get; set; } // For Option Labels in Dropdowns
    }
}
