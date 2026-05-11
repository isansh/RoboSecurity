using RoboSecurity.DTOs;

namespace RoboSecurity.Services.Interfaces
{
    public interface IRobotsService
    {
        List<RobotResponse> GetAll();
        RobotResponse? GetByName(string name);
        RobotResponse? GetById(int id);
        List<RobotResponse> GetByUserId(int userId);
        bool PostNew(string roboName, string roboIpAdress, int userId, string streamUrl, string status);
        bool DeleteById(int id);
        bool EditRobotsDetails(RobotResponse response);
    }
}
