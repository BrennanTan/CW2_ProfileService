using System.ComponentModel.DataAnnotations;

namespace CW2_ProfileService.Model
{
    public class HikingHistory
    {
        [Key]
        public int historyID { get; set; }
        public int trailID { get; set; }
        public int userID { get; set; }
        public String hikeDate { get; set; }
        public float distance { get; set; }
        public String duration { get; set; }
        public float elevation { get; set; }
    }
}
