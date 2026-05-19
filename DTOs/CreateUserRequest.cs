namespace RoboSecurity.DTOs
{
    public class CreateUserRequest
    {
        public string UserMail { get; set; }

        public List<string> UserRoles { get; set; }

        public string Password { get; set; }

        public string ConfirmPassword { get; set; }
    }
}
