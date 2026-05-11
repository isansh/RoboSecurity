using Microsoft.EntityFrameworkCore;
using RoboSecurity.DTOs;
using RoboSecurity.Helpers;
using RoboSecurity.Models;
using RoboSecurity.Services.Interfaces;

namespace RoboSecurity.Services
{
    public class RobotsService : IRobotsService
    {
        private readonly DBContext dbContext;

        public RobotsService(DBContext db)
        {
            dbContext = db;
        }

        public List<RobotResponse> GetAll()
        {
            return dbContext.Robot
                .Select(robo => new RobotResponse
                {
                    RoboId = robo.RoboId,
                    RoboName = robo.RoboName,
                    RoboIpAdress = robo.RoboIpAdress,
                    UserId = robo.UserId,
                    StreamUrl = robo.StreamUrl,
                    Status = robo.Status,
                })
                .ToList();
        }

        public RobotResponse? GetByName(string name)
        {
            var robo = dbContext.Robot
                .FirstOrDefault(u => u.RoboName == name);

            if (robo == null)
            {
                return null;
            }

            return new RobotResponse
            {
                RoboId = robo.RoboId,
                RoboName = robo.RoboName,
                RoboIpAdress = robo.RoboIpAdress,
                UserId = robo.UserId,
                StreamUrl = robo.StreamUrl,
                Status = robo.Status,
            };
        }

        public List<RobotResponse> GetByUserId(int userId)
        {
            return dbContext.Robot
                .Where(robot => robot.UserId == userId)
                .Select(robot => new RobotResponse
                {
                    RoboId = robot.RoboId,
                    RoboName = robot.RoboName,
                    RoboIpAdress = robot.RoboIpAdress,
                    StreamUrl = robot.StreamUrl,
                    Status = robot.Status,
                    UserId = robot.UserId
                })
                .ToList();
        }

        public RobotResponse? GetById(int id)
        {
            return dbContext.Robot
                .Where(r => r.RoboId == id)
                .Select(robot => new RobotResponse
                {
                    RoboId = robot.RoboId,
                    RoboName = robot.RoboName,
                    RoboIpAdress = robot.RoboIpAdress,
                    StreamUrl = robot.StreamUrl,
                    Status = robot.Status,
                    UserId = robot.UserId
                })
                .FirstOrDefault();
        }

        public bool PostNew(string roboName, string roboIpAdress, int userId, string streamUrl, string status)
        {
            var exists = dbContext.User.Any(u => u.UserId == userId);

            if (!exists)
            {
                return false;
            }

            if (!VerifyingRequestHelper.VerifyQuery(roboName) ||
                !VerifyingRequestHelper.VerifyQuery(roboIpAdress) ||
                !VerifyingRequestHelper.VerifyQuery(streamUrl) ||
                !VerifyingRequestHelper.VerifyQuery(status))
            {
                return false;
            }

            dbContext.Robot.Add(new RobotsModel
            {
                RoboName = roboName,
                UserId = userId,
                RoboIpAdress = roboIpAdress,
                StreamUrl = streamUrl,
                Status = status,
            });

            dbContext.SaveChanges();

            return true;
        }

        public bool DeleteById(int id)
        {
            var robo = dbContext.Robot.Find(id);

            if (robo == null)
            {
                return false;
            }

            dbContext.Robot.Remove(robo);

            dbContext.SaveChanges();

            return true;
        }

        public bool EditRobotsDetails(RobotResponse response)
        {
            var robo = dbContext.Robot.Find(response.RoboId);

            if (robo == null)
            {
                return false;
            }

            if (!dbContext.User.Any(r => r.UserId == response.UserId))
            {
                return false;
            }

            if (!VerifyingRequestHelper.VerifyQuery(response.RoboName) ||
                !VerifyingRequestHelper.VerifyQuery(response.RoboIpAdress) ||
                !VerifyingRequestHelper.VerifyQuery(response.StreamUrl) ||
                !VerifyingRequestHelper.VerifyQuery(response.Status))
            {
                return false;
            }

            robo.RoboName = response.RoboName;
            robo.RoboIpAdress = response.RoboIpAdress;
            robo.UserId = response.UserId;
            robo.StreamUrl = response.StreamUrl;
            robo.Status = response.Status;

            dbContext.SaveChanges();

            return true;
        }
    }
}
