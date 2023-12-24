using System.ComponentModel.DataAnnotations;

namespace CW2_ProfileService.Model
{
    public class Trails
    {
        [Key]
        public int TrailID { get; set; }
        public String TrailName { get; set; }
    }
}
