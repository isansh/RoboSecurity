namespace RoboSecurity.DTOs
{
    public class AlarmResponse
    {
        public int AlarmId { get; set; }
        public int RoboId { get; set; }
        public string RoboName { get; set; }
        public DateTime Timestamp { get; set; }
        public string Message { get; set; }
        public bool IsResolved { get; set; }

        public string UserEmail { get; set; } = null!;
        public string UserPhone { get; set; } = null!;
    }
}