using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace RoboSecurity.Models
{
    [Table("Users")]
    public class UsersModel
    {
        [Key]
        [Column("user_id")]
        public int UserId { get; set; }

        [Column("user_mail")]
        public string UserMail { get; set; }

        [Column("user_password")]
        public string UserPassword { get; set; }

        public virtual ICollection<UserRolesModel> UserRoles { get; set; } = new List<UserRolesModel>();
    }
}