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
        public int? UserId { get; set; }

        [JsonIgnore]
        [ForeignKey("UserId")]
        public UsersModel? User { get; set; }

        [Required]
        [Column("secret_token")]
        public string SecretToken { get; set; }

        [Column("status")]
        public string Status { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
