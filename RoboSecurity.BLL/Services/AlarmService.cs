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
                .Include(a => a.Robot)
                .ThenInclude(r => r.User)
                .OrderByDescending(a => a.Timestamp)
                .Select(a => MapToAlarmResponse(a))
                .ToListAsync();
        }

        public async Task<List<AlarmResponse>> GetUnresolvedAlarms()
        {
            return await dbContext.Alarms
                .Include(a => a.Robot)
                .ThenInclude(r => r.User)
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
                .Include(a => a.Robot)
                .ThenInclude(r => r.User)
                .Where(a => a.Timestamp >= fromDate && a.Timestamp <= toDate)
                .OrderByDescending(a => a.Timestamp)
                .Select(a => MapToAlarmResponse(a))
                .ToListAsync();
        }

        public async Task<bool> CreateAlarm(AlarmRequest request)
        {
            var robot = await dbContext.Robot.FirstOrDefaultAsync(r => r.Token == request.SecretToken);
            if (robot == null) return false;

            var cooldownPeriod = DateTime.UtcNow.AddSeconds(-300);
            bool isSpam = await dbContext.Alarms.AnyAsync(a =>
                a.RoboId == robot.RoboId &&
                a.Timestamp > cooldownPeriod &&
                a.IsResolved == false
            );

            if (isSpam) return true;

            string relativeUrlPath = string.Empty;
            if (!string.IsNullOrEmpty(request.ImageBase64))
            {
                try
                {
                    byte[] imageBytes = Convert.FromBase64String(request.ImageBase64);

                    string alarmsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "alarms");

                    if (!Directory.Exists(alarmsFolder))
                        Directory.CreateDirectory(alarmsFolder);

                    string uniqueFileName = $"{Guid.NewGuid()}_{request.ImageName}";
                    string filePath = Path.Combine(alarmsFolder, uniqueFileName);

                    await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);

                    relativeUrlPath = $"/uploads/alarms/{uniqueFileName}";
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Сервіс Охорони] Помилка обробки файлу: {ex.Message}");
                    return false;
                }
            }

            var newAlarm = new AlarmModel
            {
                RoboId = robot.RoboId,
                Percent = request.Percent,
                Timestamp = DateTime.UtcNow,
                IsResolved = false,
                SnapshotPath = relativeUrlPath
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