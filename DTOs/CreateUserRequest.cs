namespace RoboSecurity.DTOs
{
    public class CreateUserRequest
    {
        public string UserMail { get; set; }

        public string UserRoleName { get; set; }

        public string Password { get; set; }

        public string ConfirmPassword { get; set; }
    }
}
