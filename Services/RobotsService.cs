using Azure.Core;
using Microsoft.AspNetCore.Mvc;
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
                    UserId = robo.UserId,
                    Status = robo.Status,
                    CreatedAt = robo.CreatedAt,
                    SecretToken = null
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
                UserId = robo.UserId,
                Status = robo.Status,
                CreatedAt = robo.CreatedAt,
                SecretToken = robo.SecretToken
            };
        }

        public List<RobotResponse> GetByUserId(int userId)
        {
            return dbContext.Robot
                .Where(robo => robo.UserId == userId)
                .Select(robo => new RobotResponse
                {
                    RoboId = robo.RoboId,
                    RoboName = robo.RoboName,
                    UserId = robo.UserId,
                    Status = robo.Status,
                    CreatedAt = robo.CreatedAt,
                    SecretToken = null
                })
                .ToList();
        }

        public RobotResponse? GetById(int id)
        {
            return dbContext.Robot
                .Where(r => r.RoboId == id)
                .Select(robo => new RobotResponse
                {
                    RoboId = robo.RoboId,
                    RoboName = robo.RoboName,
                    UserId = robo.UserId,
                    Status = robo.Status,
                    CreatedAt = robo.CreatedAt,
                    SecretToken = robo.SecretToken
                })
                .FirstOrDefault();
        }

        public bool PostNew(CreateRobotRequest request)
        {
            if (request == null || !VerifyingRequestHelper.VerifyQuery(request.RoboName))
            {
                return false;
            }

            if (request.UserId.HasValue && !dbContext.User.Any(u => u.UserId == request.UserId.Value))
            {
                return false;
            }

            string generatedToken = "tok_" + Guid.NewGuid().ToString("N");

            var newRobot = new RobotsModel
            {
                RoboName = request.RoboName,
                UserId = request.UserId,
                SecretToken = generatedToken,
                Status = "pending_activation",
                CreatedAt = DateTime.UtcNow
            };

            dbContext.Robot.Add(newRobot);
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
            if (robo == null) return false;

            if (response.UserId.HasValue && !dbContext.User.Any(r => r.UserId == response.UserId.Value))
            {
                return false;
            }

            if (!VerifyingRequestHelper.VerifyQuery(response.RoboName) ||
                !VerifyingRequestHelper.VerifyQuery(response.Status))
            {
                return false;
            }

            robo.RoboName = response.RoboName;
            robo.UserId = response.UserId;
            robo.Status = response.Status;

            if (!string.IsNullOrEmpty(response.SecretToken))
            {
                robo.SecretToken = response.SecretToken;
            }

            dbContext.SaveChanges();
            return true;
        }

        public bool ActivateRobot(RobotActivationDto request)
        {
            if (request == null || string.IsNullOrEmpty(request.SecretToken))
            {
                return false;
            }

            var robot = dbContext.Robot.FirstOrDefault(r => r.SecretToken == request.SecretToken);

            if (robot == null)
            {
                return false;
            }

            robot.Status = "active";

            dbContext.SaveChanges();
            return true;
        }
    }
}
