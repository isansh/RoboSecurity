using RoboSecurity.DTOs;

namespace RoboSecurity.Services.Interfaces
{
    public interface IUsersService
    {
        List<UserResponse> GetAll();
        UserResponse? GetByMail(string mail);
        bool PostNew(string mail, string password, string confirmPassword, string role);
        bool DeleteById(int id);
        bool EditUserDetails(ChangeUserRequest change);
    }
}