using RoboSecurity.DTOs;
using RoboSecurity.Models;
using RoboSecurity.Services.Interfaces;

namespace RoboSecurity.Services
{
    public class AlarmsService : IAlarmsService
    {
        private readonly DBContext dbContext;

        public AlarmsService(DBContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public List<AlarmResponse> GetAllAlarms()
        {
            return dbContext.Alarms
                .OrderByDescending(a => a.Timestamp)
                .Select(a => new AlarmResponse
                {
                    AlarmId = a.AlarmId,
                    RoboId = a.RoboId,
                    RoboName = dbContext.Robot.FirstOrDefault(r => r.RoboId == a.RoboId)!.RoboName ?? "Невідомий робот",
                    Timestamp = a.Timestamp,
                    Message = a.Message,
                    IsResolved = a.IsResolved,
                    UserEmail = dbContext.Robot.FirstOrDefault(r => r.RoboId == a.RoboId)!.User.UserMail ?? "Не вказано",
                    UserPhone = dbContext.Robot.FirstOrDefault(r => r.RoboId == a.RoboId)!.User.PhoneNumber ?? ""
                })
                .ToList();
        }

        public List<AlarmResponse> GetUnresolvedAlarms()
        {
            return dbContext.Alarms
                .Where(a => !a.IsResolved)
                .OrderByDescending(a => a.Timestamp)
                .Select(a => new AlarmResponse
                {
                    AlarmId = a.AlarmId,
                    RoboId = a.RoboId,
                    RoboName = dbContext.Robot.FirstOrDefault(r => r.RoboId == a.RoboId)!.RoboName ?? "Невідомий робот",
                    Timestamp = a.Timestamp,
                    Message = a.Message,
                    IsResolved = a.IsResolved,
                    UserEmail = dbContext.Robot.FirstOrDefault(r => r.RoboId == a.RoboId)!.User.UserMail ?? "Не вказано",
                    UserPhone = dbContext.Robot.FirstOrDefault(r => r.RoboId == a.RoboId)!.User.PhoneNumber ?? ""
                })
                .ToList();
        }

        public bool CreateAlarm(AlarmRequest request)
        {
            var robotExists = dbContext.Robot.Any(r => r.RoboId == request.RoboId);
            if (!robotExists)
            {
                return false;
            }

            var newAlarm = new AlarmModel
            {
                RoboId = request.RoboId,
                Message = request.Message,
                Timestamp = DateTime.UtcNow,
                IsResolved = false
            };

            dbContext.Alarms.Add(newAlarm);
            dbContext.SaveChanges();
            return true;
        }

        public bool ResolveAlarm(int alarmId)
        {
            var alarm = dbContext.Alarms.Find(alarmId);
            if (alarm == null)
            {
                return false;
            }

            alarm.IsResolved = true;
            dbContext.SaveChanges();

            return true;
        }

        public List<AlarmResponse> GetAlarmsByDate(DateTime fromDate, DateTime toDate)
        {
            if (toDate.Hour == 0 && toDate.Minute == 0)
            {
                toDate = toDate.Date.AddDays(1).AddTicks(-1);
            }

            return dbContext.Alarms
                .Where(a => a.Timestamp >= fromDate && a.Timestamp <= toDate)
                .OrderByDescending(a => a.Timestamp)
                .Select(a => new AlarmResponse
                {
                    AlarmId = a.AlarmId,
                    RoboId = a.RoboId,
                    RoboName = dbContext.Robot.FirstOrDefault(r => r.RoboId == a.RoboId)!.RoboName ?? "Невідомий робот",
                    Timestamp = a.Timestamp,
                    Message = a.Message,
                    IsResolved = a.IsResolved,
                    UserEmail = dbContext.Robot.FirstOrDefault(r => r.RoboId == a.RoboId)!.User.UserMail ?? "Не вказано",
                    UserPhone = dbContext.Robot.FirstOrDefault(r => r.RoboId == a.RoboId)!.User.PhoneNumber ?? ""
                })
                .ToList();
        }
    }
}
