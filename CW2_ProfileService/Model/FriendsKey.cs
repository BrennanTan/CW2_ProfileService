using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace CW2_ProfileService.Model
{
    public class FriendsKey
    {
        [Key]
        [SwaggerSchema(ReadOnly = true)]
        public int friendsKeyID { get; set; }
        public int userID { get; set; }
        public int friendID { get; set; }
    }
} 
