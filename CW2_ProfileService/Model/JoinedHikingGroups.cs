using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace CW2_ProfileService.Model
{
    public class JoinedHikingGroups
    {
        [Key]
        [SwaggerSchema(ReadOnly = true)]
        public int joinedGroupID { get; set; }
        public int groupID { get; set; }
        public int userID { get; set; }
    }
}
