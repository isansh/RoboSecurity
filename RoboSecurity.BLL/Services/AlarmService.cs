using Microsoft.EntityFrameworkCore;
using RoboSecurity.BLL.DTOs;
using RoboSecurity.DAL.Models;
using RoboSecurity.BLL.Services.Interfaces;

namespace RoboSecurity.BLL.Services
{
    public class AlarmsService : IAlarmsService
    {
        private readonly DBContext dbContext;

        public AlarmsService(DBContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<List<AlarmResponse>> GetAllAlarms()
        {
            return await dbContext.Alarms
                .OrderByDescending(a => a.Timestamp)
                .Select(a => MapToAlarmResponse(a))
                .ToListAsync();
        }

        public async Task<List<AlarmResponse>> GetUnresolvedAlarms()
        {
            return await dbContext.Alarms
                .Where(a => !a.IsResolved)
                .OrderByDescending(a => a.Timestamp)
                .Select(a => MapToAlarmResponse(a))
                .ToListAsync();
        }

        public async Task<List<AlarmResponse>> GetAlarmsByDate(DateTime fromDate, DateTime toDate)
        {
            if (toDate.Hour == 0 && toDate.Minute == 0)
            {
                toDate = toDate.Date.AddDays(1).AddTicks(-1);
            }

            return await dbContext.Alarms
                .Where(a => a.Timestamp >= fromDate && a.Timestamp <= toDate)
                .OrderByDescending(a => a.Timestamp)
                .Select(a => MapToAlarmResponse(a))
                .ToListAsync();
        }

        public async Task<bool> CreateAlarm(AlarmRequest request, string relativePath)
        {
            var robot = await dbContext.Robot.FirstOrDefaultAsync(r => r.Token == request.SecretToken);
            if (robot == null) return false;

            var cooldownPeriod = DateTime.UtcNow.AddSeconds(-3000);
            bool isSpam = await dbContext.Alarms.AnyAsync(a =>
                a.RoboId == robot.RoboId &&
                a.Timestamp > cooldownPeriod &&
                a.IsResolved == false
            );

            if (isSpam) return true;

            var newAlarm = new AlarmModel
            {
                RoboId = robot.RoboId,
                Percent = request.Percent,
                Timestamp = DateTime.UtcNow,
                IsResolved = false,
                SnapshotPath = relativePath
            };

            dbContext.Alarms.Add(newAlarm);
            await dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ResolveAlarm(int alarmId)
        {
            var alarm = await dbContext.Alarms.FindAsync(alarmId);
            if (alarm == null) return false;

            alarm.IsResolved = true;
            await dbContext.SaveChangesAsync();
            return true;
        }

        private static AlarmResponse MapToAlarmResponse(AlarmModel a)
        {
            return new AlarmResponse
            {
                AlarmId = a.AlarmId,
                RoboId = a.RoboId,
                RoboName = a.Robot != null ? a.Robot.RoboName : "Невідомий робот",
                Timestamp = a.Timestamp,
                Message = a.Percent,
                IsResolved = a.IsResolved,
                SnapshotPath = a.SnapshotPath,
                UserEmail = (a.Robot != null && a.Robot.User != null) ? a.Robot.User.UserMail : "Не вказано",
                UserPhone = (a.Robot != null && a.Robot.User != null) ? a.Robot.User.PhoneNumber : ""
            };
        }
    }
}