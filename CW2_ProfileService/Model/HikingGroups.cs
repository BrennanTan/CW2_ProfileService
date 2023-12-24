using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace CW2_ProfileService.Model
{
    public class HikingGroups
    {
        [Key]
        [SwaggerSchema(ReadOnly = true)]
        public int groupID { get; set; }
        public int creatorUserId { get; set; }
        public String groupName { get; set; }
        public String description { get; set; }
    }
}
