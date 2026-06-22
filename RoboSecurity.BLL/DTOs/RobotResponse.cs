namespace RoboSecurity.BLL.DTOs
{
    public class RobotResponse
    {
        public int RoboId { get; set; }

        public string RoboName { get; set; }

        public int UserId { get; set; }

        public string Status { get; set; }

        public string? Token { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
