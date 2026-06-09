namespace RoboSecurity.BLL.DTOs
{
    public class UserResponse
    {
        public int UserId { get; set; }

        public string UserMail { get; set; }

        public string PhoneNumber { get; set; }

        public string UserPassword { get; set; }

        public List<string> UserRoles { get; set; }
    }
}
