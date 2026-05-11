using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace RoboSecurity.Models
{
    [Table("Robots")]
    public class RobotsModel
    {
        [Key]
        [Column("robo_id")]
        public int RoboId { get; set; }

        [Column("robo_name")]
        public string RoboName { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [JsonIgnore]
        [ForeignKey("UserId")]
        public UsersModel User { get; set; }

        [Column("stream_url")]
        public string StreamUrl { get; set; }

        [Column("status")]
        public string Status { get; set; }

        [Column("robo_ip_adress")]
        public string RoboIpAdress { get; set; }
    }
}
