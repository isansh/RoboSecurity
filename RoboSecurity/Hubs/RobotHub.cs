using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RoboSecurity.DAL.Models;

namespace RoboSecurity.Hubs
{
    public class RobotHub : Hub
    {
        private readonly DBContext dbContext;
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public RobotHub(DBContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task RegisterRobot(string secretToken)
        {
            if (string.IsNullOrEmpty(secretToken))
            {
                Context.Abort();
                return;
            }

            await _semaphore.WaitAsync();
            try
            {
                var robot = await dbContext.Robot.FirstOrDefaultAsync(r => r.Token == secretToken);

                if (robot == null)
                {
                    Console.WriteLine($"[SignalR Hub] Спроба підключення з невірним токеном.");
                    Context.Abort();
                    return;
                }

                Context.Items["RobotId"] = robot.RoboId;

                await Groups.AddToGroupAsync(Context.ConnectionId, $"Robot_{robot.RoboId}");

                robot.Status = "active";
                await dbContext.SaveChangesAsync();

                Console.WriteLine($"[SignalR Hub] Робот {robot.RoboName} (ID: {robot.RoboId}) успішно підключився.");

                await Clients.Caller.SendAsync("RegistrationConfirmed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SignalR Hub] Помилка при реєстрації робота: {ex.Message}");
                Context.Abort();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task UploadVideoFrame(string frameBase64)
        {
            if (Context.Items.TryGetValue("RobotId", out var robotIdObj) && robotIdObj is int robotId)
            {
                await Clients.Group($"Watchers_{robotId}").SendAsync("ReceiveFrame", frameBase64);
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await _semaphore.WaitAsync();
            try
            {
                if (Context.Items.TryGetValue("RobotId", out var robotIdObj) && robotIdObj is int robotId)
                {
                    var robot = await dbContext.Robot.FindAsync(robotId);
                    if (robot != null && robot.Status == "active")
                    {
                        robot.Status = "offline";
                        await dbContext.SaveChangesAsync();
                        Console.WriteLine($"[SignalR Hub] Робот ID {robotId} відключився.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SignalR Hub] Помилка при відключенні робота: {ex.Message}");
            }
            finally
            {
                _semaphore.Release();
                await base.OnDisconnectedAsync(exception);
            }
        }

        public async Task StartWatchingRobot(int robotId)
        {
            if (Context.User?.Identity?.IsAuthenticated != true)
            {
                throw new HubException("Unauthorized");
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, $"Watchers_{robotId}");
        }

        public async Task StopWatchingRobot(int robotId)
        {
            if (Context.User?.Identity?.IsAuthenticated != true)
            {
                throw new HubException("Unauthorized");
            }

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Watchers_{robotId}");
        }
    }
}