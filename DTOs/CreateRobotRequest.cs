using System.ComponentModel.DataAnnotations;

namespace RoboSecurity.DTOs
{
    public class CreateRobotRequest
    {
        [Required]
        public string RoboName { get; set; }

        public int? UserId { get; set; }
    }
}
