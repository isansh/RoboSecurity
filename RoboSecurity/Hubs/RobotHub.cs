using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RoboSecurity.DAL.Models;

namespace RoboSecurity.Hubs
{
    public class RobotHub : Hub
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public RobotHub(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task RegisterRobot(string secretToken)
        {
            if (string.IsNullOrEmpty(secretToken))
            {
                Context.Abort();
                return;
            }

            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<DBContext>();

                try
                {
                    var robot = await dbContext.Robot.FirstOrDefaultAsync(r => r.Token == secretToken);

                    if (robot == null)
                    {
                        Console.WriteLine($"[SignalR Hub] Спроба підключення з невірним токеном.");
                        Context.Abort();
                        return;
                    }

                    string currentStatus = robot.Status?.Trim().ToLower() ?? "";

                    Context.Items["RobotId"] = robot.RoboId;
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"Robot_{robot.RoboId}");

                    if (currentStatus == "watchdog")
                    {
                        Console.WriteLine($"[SignalR Hub] Робот {robot.RoboName} (ID: {robot.RoboId}) підключився. Режим охорони [watchdog] АКТИВНИЙ.");
                    }
                    else
                    {
                        robot.Status = "active";
                        await dbContext.SaveChangesAsync();
                        Console.WriteLine($"[SignalR Hub] Робот {robot.RoboName} (ID: {robot.RoboId}) увійшов в мережу. Статус: active");
                    }

                    await Clients.Caller.SendAsync("RegistrationConfirmed");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[SignalR Hub] Помилка при реєстрації робота: {ex.Message}");
                    Context.Abort();
                }
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
            if (Context.Items.TryGetValue("RobotId", out var robotIdObj) && robotIdObj is int robotId)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<DBContext>();

                    try
                    {
                        var robot = await dbContext.Robot.FirstOrDefaultAsync(r => r.RoboId == robotId);

                        if (robot != null)
                        {
                            string currentStatus = robot.Status?.Trim().ToLower() ?? "";

                            if (currentStatus == "watchdog")
                            {
                                Console.WriteLine($"[SignalR Hub] РОБОТ В ОХОРОНІ ОФЛАЙН! Статус залишається watchdog.");
                            }
                            else
                            {
                                Console.WriteLine($"[SignalR Hub] Робот {robot.RoboName} відключився. Змінюємо статус на offline.");
                                robot.Status = "offline";
                                await dbContext.SaveChangesAsync();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[SignalR Hub] Помилка при відключенні робота: {ex.Message}");
                    }
                }
            }
            await base.OnDisconnectedAsync(exception);
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