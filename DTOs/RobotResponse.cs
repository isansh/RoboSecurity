using RoboSecurity.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace RoboSecurity.DTOs
{
    public class RobotResponse
    {
        public int RoboId { get; set; }

        public string RoboName { get; set; }

        public string RoboIpAdress { get; set; }

        public int UserId { get; set; }

        public string StreamUrl { get; set; }

        public string Status { get; set; }
    }
}
