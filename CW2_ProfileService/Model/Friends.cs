using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace CW2_ProfileService.Model
{
    public class Friends
    {
        [Key]
        [SwaggerSchema(ReadOnly = true)]
        public int friendID { get; set; }
        public int receiverID { get; set; }
        public int senderID { get; set; }
        public String friendStatus { get; set; }
    }
}
