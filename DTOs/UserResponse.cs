using System.Text.Json.Serialization;

namespace RoboSecurity.DTOs
{
    public class UserResponse
    {
        public int UserId { get; set; }

        public string UserMail { get; set; }

        public string UserPassword { get; set; }

        public string UserRoleName { get; set; }
    }
}
