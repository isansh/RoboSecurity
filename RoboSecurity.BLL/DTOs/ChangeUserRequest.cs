namespace RoboSecurity.BLL.DTOs
{
    public class ChangeUserRequest
    {
        public int UserId { get; set; }

        public string UserMail { get; set; }

        public string PhoneNumber { get; set; }

        public List<string> UserRoles { get; set; }

        public string Password { get; set; }

        public string ConfirmPassword { get; set; }
    }
}
