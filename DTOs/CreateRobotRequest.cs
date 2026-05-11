namespace RoboSecurity.DTOs
{
    public class CreateRobotRequest
    {
        public string RoboName { get; set; }

        public int UserId { get; set; }

        public string RoboIpAdress { get; set; }

        public string StreamUrl { get; set; }

        public string Status { get; set; }
    }
}
