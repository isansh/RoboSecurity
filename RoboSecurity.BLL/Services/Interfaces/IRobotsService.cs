using RoboSecurity.BLL.DTOs;

namespace RoboSecurity.BLL.Services.Interfaces
{
    public interface IRobotsService
    {
        Task<bool> CheckUserAccessToRobot(int robotId, int userId);
        Task<List<RobotResponse>> GetAll();
        Task<RobotResponse?> GetByName(string name);
        Task<RobotResponse?> GetById(int id);
        Task<RobotResponse?> GetByToken(string token);
        Task<List<RobotResponse>> GetByUserId(int userId);
        Task<bool> PostNew(CreateRobotRequest request);
        Task<bool> DeleteById(int id);
        Task<bool> EditRobotsDetails(RobotResponse response);
    }
}
