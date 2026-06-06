using RoboSecurity.Services.Interfaces;
using System.Collections.Concurrent;

namespace RoboSecurity.Services
{
    public class RobotConnectionService : IRobotConnectionService
    {
        private readonly ConcurrentDictionary<int, string> RobotAddresses = new ConcurrentDictionary<int, string>();

        public void RegisterRobotIp(int robotId, string ipAddress)
        {
            RobotAddresses[robotId] = ipAddress;
        }

        public string? GetRobotIp(int robotId)
        {
            if (RobotAddresses.TryGetValue(robotId, out var ip))
            {
                return ip;
            }
            return null;
        }

        public async Task<bool> SendControlCommandAsync(int robotId, string action)
        {
            var robotIp = GetRobotIp(robotId);
            if (string.IsNullOrEmpty(robotIp))
            {
                return false;
            }

            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(2);

            string flaskUrl = $"http://{robotIp}:5000/control?action={action}";

            try
            {
                var response = await client.GetAsync(flaskUrl);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<Stream?> GetRobotVideoReaderAsync(int robotId)
        {
            var robotIp = GetRobotIp(robotId);

            if (string.IsNullOrEmpty(robotIp))
            {
                return null;
            }

            string raspberryVideoUrl = $"http://{robotIp}:5000/video_feed";
            var client = new HttpClient();

            try
            {
                var response = await client.GetAsync(raspberryVideoUrl, HttpCompletionOption.ResponseHeadersRead);

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                return await response.Content.ReadAsStreamAsync();
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
