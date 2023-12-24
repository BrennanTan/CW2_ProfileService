using System.ComponentModel.DataAnnotations;

namespace CW2_ProfileService.Model
{
    public class UserProfile
    {
        [Key]
        public int UserID { get; set; }
        public String Username { get; set; }
        public String Password { get; set; }
        public String Email { get; set; }
        public String Role { get; set; }
        public String JoinDate { get; set; }
        public String Status { get; set; }
    }

}
