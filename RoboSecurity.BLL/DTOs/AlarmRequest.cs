namespace RoboSecurity.BLL.DTOs
{
    public class AlarmRequest
    {
        public string SecretToken { get; set; }
        public string Percent { get; set; }
        public string ImageName { get; set; }
        public string ImageBase64 { get; set; }
    }
}