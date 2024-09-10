using System.ComponentModel.DataAnnotations;

namespace Skillbridge.Business.Model.Db
{
    public class APIStateModel
    {
        [Key]
        public int Id { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string TokenType { get; set; }
        public int ExpiresIn { get; set; }
        public DateTime ExpirationDate { get; set; }
    }
}
