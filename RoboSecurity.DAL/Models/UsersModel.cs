using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace RoboSecurity.DAL.Models
{
    [Table("Users")]
    public class UsersModel
    {
        [Key]
        [Column("user_id")]
        public int UserId { get; set; }

        [Column("mail")]
        public string UserMail { get; set; }

        [Column("phone_number")]
        public string PhoneNumber { get; set; }

        [Column("password")]
        public string UserPassword { get; set; }

        public virtual ICollection<UserRolesModel> UserRoles { get; set; } = new List<UserRolesModel>();
    }
}