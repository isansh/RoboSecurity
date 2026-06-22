using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoboSecurity.BLL.DTOs;
using RoboSecurity.BLL.Helpers;
using RoboSecurity.DAL.Models;
using RoboSecurity.BLL.Services.Interfaces;

namespace RoboSecurity.BLL.Services
{
    public class RobotsService : IRobotsService
    {
        private readonly DBContext dbContext;

        public RobotsService(DBContext db)
        {
            dbContext = db;
        }

        public async Task<bool> CheckUserAccessToRobot(int robotId, int userId)
        {
            return await dbContext.Robot.AnyAsync(r => r.RoboId == robotId && r.UserId == userId);
        }

        public async Task<List<RobotResponse>> GetAll()
        {
            var robots = await dbContext.Robot.ToListAsync();
            return robots.Select(r => MapToRobotResponse(r, includeToken: false)).ToList();
        }

        public async Task<RobotResponse?> GetByName(string name)
        {
            {
                var robo = await dbContext.Robot.FirstOrDefaultAsync(u => u.RoboName == name);
                if (robo == null) return null;

                return MapToRobotResponse(robo, includeToken: true);
            }
        }

        public async Task<List<RobotResponse>> GetByUserId(int userId)
        {
            var robots = await dbContext.Robot
                .Where(robo => robo.UserId == userId)
                .ToListAsync();

            return robots.Select(r => MapToRobotResponse(r, includeToken: false)).ToList();
        }

        public async Task<RobotResponse?> GetById(int id)
        {
            var robo = await dbContext.Robot.FirstOrDefaultAsync(r => r.RoboId == id);
            if (robo == null) return null;

            return MapToRobotResponse(robo, includeToken: true);
        }

        public async Task<RobotResponse?> GetByToken(string token)
        {
            var robo = await dbContext.Robot.FirstOrDefaultAsync(u => u.Token == token);
            if (robo == null) return null;

            return MapToRobotResponse(robo, includeToken: true);
        }

        public async Task<bool> PostNew(CreateRobotRequest request)
        {
            if (request == null || !VerifyingRequestHelper.VerifyQuery(request.RoboName))
            {
                return false;
            }

            var userExists = await dbContext.User.AnyAsync(u => u.UserId == request.UserId);
            if (!userExists)
            {
                return false;
            }

            var hasUserRole = await dbContext.UserRoles.AnyAsync(ur =>
                                    ur.UserId == request.UserId &&
                                    ur.Role.RoleName == "user");

            if (!hasUserRole)
            {
                return false;
            }

            string generatedToken = "tok_" + Guid.NewGuid().ToString("N");

            var newRobot = new RobotsModel
            {
                RoboName = request.RoboName,
                UserId = request.UserId,
                Token = generatedToken,
                Status = "pending_activation",
                CreatedAt = DateTime.UtcNow
            };

            dbContext.Robot.Add(newRobot);
            await dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteById(int id)
        {
            var robo = await dbContext.Robot.FindAsync(id);
            if (robo == null) return false;

            var linkedAlarms = await dbContext.Alarms.Where(a => a.RoboId == id).ToListAsync();
            if (linkedAlarms.Any())
            {
                dbContext.Alarms.RemoveRange(linkedAlarms);
            }

            dbContext.Robot.Remove(robo);
            await dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EditRobotsDetails(RobotResponse response)
        {
            var robo = await dbContext.Robot.FindAsync(response.RoboId);
            if (robo == null) return false;

            if (!await dbContext.User.AnyAsync(r => r.UserId == response.UserId))
            {
                return false;
            }

            if (!VerifyingRequestHelper.VerifyQuery(response.RoboName))
            {
                return false;
            }

            robo.RoboName = response.RoboName;

            if (VerifyingRequestHelper.VerifyQuery(response.Status))
            {
                string incomingStatus = response.Status.Trim().ToLower();

                if (incomingStatus == "watchdog" || incomingStatus == "active" || incomingStatus == "offline")
                {
                    robo.Status = incomingStatus;
                }
            }
            else
            {
                return false;
            }

            await dbContext.SaveChangesAsync();
            return true;
        }

        private static RobotResponse MapToRobotResponse(RobotsModel robo, bool includeToken)
        {
            return new RobotResponse
            {
                RoboId = robo.RoboId,
                RoboName = robo.RoboName,
                UserId = robo.UserId,
                Status = robo.Status,
                CreatedAt = robo.CreatedAt,
                Token = includeToken ? robo.Token : null
            };
        }
    }
}
