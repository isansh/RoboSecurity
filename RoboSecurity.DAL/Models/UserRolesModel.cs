using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace RoboSecurity.DAL.Models
{
    [Table("UserRoles")]
    public class UserRolesModel
    {
        [Column("user_id")]
        public int UserId { get; set; }

        [Column("role_id")]
        public int RoleId { get; set; }

        [JsonIgnore]
        [ForeignKey("UserId")]
        public UsersModel User {  get; set; }

        [JsonIgnore]
        [ForeignKey("RoleId")]
        public RolesModel Role { get; set; }
    }
}