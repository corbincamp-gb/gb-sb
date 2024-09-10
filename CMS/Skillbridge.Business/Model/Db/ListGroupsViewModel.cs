namespace Skillbridge.Business.Model.Db
{
    public class ListGroupsViewModel
    {
        public int Group_Id { get; set; }   // This is the number that will appear in the data on the site as groupid
        public string Title { get; set; }
        public double Lat { get; set; }
        public double Long { get; set; }
        public List<int> Opportunities { get; set; }
    }
}
