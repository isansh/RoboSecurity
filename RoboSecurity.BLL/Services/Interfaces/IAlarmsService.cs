using Microsoft.AspNetCore.Http;
using RoboSecurity.BLL.DTOs;

namespace RoboSecurity.BLL.Services.Interfaces
{
    public interface IAlarmsService
    {
        Task<List<AlarmResponse>> GetAllAlarms();
        Task<List<AlarmResponse>> GetUnresolvedAlarms();
        Task<bool> CreateAlarm(AlarmRequest request, string relativePath);
        Task<bool> ResolveAlarm(int alarmId);
        Task<List<AlarmResponse>> GetAlarmsByDate(DateTime fromDate, DateTime toDate);
    }
}
