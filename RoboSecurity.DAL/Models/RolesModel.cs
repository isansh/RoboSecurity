using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoboSecurity.DAL.Models
{
    [Table("Roles")]
    public class RolesModel
    {
        [Key]
        [Column("role_id")]
        public int RoleId { get; set; }

        [Column("name")]
        public string RoleName { get; set; }
    }
}