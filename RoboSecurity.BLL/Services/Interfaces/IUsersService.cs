using RoboSecurity.BLL.DTOs;
using RoboSecurity.DAL.Models;

namespace RoboSecurity.BLL.Services.Interfaces
{
    public interface IUsersService
    {
        Task<List<UserResponse>> GetAll();
        Task<UserResponse?> GetByMail(string mail);
        Task<bool> PostNew(CreateUserRequest create);
        Task<bool> DeleteById(int id);
        Task<bool> EditUserDetails(ChangeUserRequest change);
        Task<UsersModel?> GetRawByMail(string mail);
    }
}