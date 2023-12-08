using System.ComponentModel.DataAnnotations;

namespace itransition4.Models
{
    public class Users
    {
        [Key]
        public int Id_user { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public DateTime Data_reg { get; set; }
        public DateTime Last_data { get; set; }
        public string Dostup { get; set; }
    }

    //public enum Dostup
    //{
    //    Active,
    //    Deleted,
    //    Blocked
    //}
}
