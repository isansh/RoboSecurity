using RoboSecurity.DTOs;

namespace RoboSecurity.Services.Interfaces
{
    public interface IAlarmsService
    {
        List<AlarmResponse> GetAllAlarms();
        List<AlarmResponse> GetUnresolvedAlarms();
        bool CreateAlarm(AlarmRequest request);
        bool ResolveAlarm(int alarmId);
        List<AlarmResponse> GetAlarmsByDate(DateTime fromDate, DateTime toDate);
    }
}
