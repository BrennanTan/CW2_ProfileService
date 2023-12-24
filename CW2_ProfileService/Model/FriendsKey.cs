using System.ComponentModel.DataAnnotations;

namespace CW2_ProfileService.Model
{
    public class FriendsKey
    {
        [Key]
        public int friendsKeyID { get; set; }
        public int userID { get; set; }
        public int friendID { get; set; }
    }
} 
