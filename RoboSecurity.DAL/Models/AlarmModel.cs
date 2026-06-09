using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace RoboSecurity.DAL.Models
{
    public class AlarmModel
    {
        [Key]
        [Column("alarm_id")]
        public int AlarmId { get; set; }

        [Column("robo_id")]
        public int RoboId { get; set; }

        [Column("timestamp")]
        public DateTime Timestamp { get; set; }

        [Column("persent")]
        public string Percent { get; set; }

        [Column("is_resolved")]
        public bool IsResolved { get; set; }

        [Column("snapshot_path")]
        public string? SnapshotPath { get; set; }

        [JsonIgnore]
        [ForeignKey("RoboId")]
        public RobotsModel Robot { get; set; }
    }
}
