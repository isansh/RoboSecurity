using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using RoboSecurity.BLL.DTOs;
using RoboSecurity.BLL.Services;
using RoboSecurity.BLL.Services.Interfaces;
using RoboSecurity.BLL.Helpers;
using RoboSecurity.Hubs;

namespace RoboSecurity.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class RobotsController : Controller
    {
        private readonly IHubContext<RobotHub> _hubContext;
        private readonly IRobotsService robotService;

        public RobotsController(IRobotsService robotService, IHubContext<RobotHub> hubContext)
        {
            ArgumentNullException.ThrowIfNull(hubContext);
            ArgumentNullException.ThrowIfNull(robotService);
            this.robotService = robotService;
            this._hubContext = hubContext;
        }

        [Authorize(Roles = "admin")]
        [HttpGet]
        public async Task<IActionResult> GetRobots()
        {
            var robo = await robotService.GetAll();
            if (robo is null) return NotFound();

            return Ok(robo);
        }

        [Authorize(Roles = "admin,user")]
        [HttpGet("id/{id}")]
        public async Task<IActionResult> GetRobotById(int id)
        {
            var robo = await robotService.GetById(id);
            if (robo == null) return NotFound();

            if (User.IsInRole("admin")) return Ok(robo);

            var currentUserId = User.GetUserId();

            if (await robotService.CheckUserAccessToRobot(robo.RoboId, currentUserId))
            {
                return Ok(robo);
            }

            return NotFound();
        }

        [Authorize(Roles = "admin,user")]
        [HttpGet("name/{name}")]
        public async Task<IActionResult> GetRobotByName(string name)
        {
            var robo = await robotService.GetByName(name);
            if (robo == null) return NotFound();

            if (User.IsInRole("admin")) return Ok(robo);

            var currentUserId = User.GetUserId();

            if (await robotService.CheckUserAccessToRobot(robo.RoboId, currentUserId))
            {
                return Ok(robo);
            }

            return NotFound();
        }

        [Authorize(Roles = "admin,user")]
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetRobotsByUserId(int userId)
        {
            var currentUserId = User.GetUserId();
            if (User.IsInRole("admin") || (currentUserId == userId))
            {
                var robots = await robotService.GetByUserId(userId);
                if (robots == null || !robots.Any()) return NotFound();

                return Ok(robots);
            }

            return NotFound();
        }

        [Authorize(Roles = "admin,user")]
        [HttpPost]
        public async Task<IActionResult> PostRobot([FromBody] CreateRobotRequest request)
        {
            if (!User.IsInRole("admin"))
            {
                var currentUserId = User.GetUserId();
                if (request.UserId != currentUserId) return BadRequest("Не можна створювати робота для іншого користувача.");
            }

            bool robo = await robotService.PostNew(request);
            if (!robo) return NotFound();

            return Ok();
        }

        [Authorize(Roles = "admin,user")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRobotById(int id)
        {
            var currentUserId = User.GetUserId();
            var isAdmin = User.IsInRole("admin");

            if (!isAdmin && !(await robotService.CheckUserAccessToRobot(id, currentUserId)))
            {
                return NotFound();
            }

            try
            {
                await _hubContext.Clients.Group($"Robot_{id}").SendAsync("ResetToFactorySettings");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Помилка відправки сигналу: {ex.Message}");
            }

            var robo = await robotService.DeleteById(id);
            if (!robo) return NotFound();

            return Ok();
        }

        [Authorize(Roles = "admin,user")]
        [HttpPut("edit")]
        public async Task<IActionResult> EditRobot([FromBody] RobotResponse change)
        {
            if (!User.IsInRole("admin"))
            {
                var currentUserId = User.GetUserId();
                if (!(await robotService.CheckUserAccessToRobot(change.RoboId, currentUserId)))
                {
                    return NotFound();
                }
            }

            var result = await robotService.EditRobotsDetails(change);
            if (!result) return NotFound();

            return Ok();
        }

        [Authorize(Roles = "admin,user")]
        [HttpPost("id/{id}/control")]
        public async Task<IActionResult> SendCommand(int id, [FromQuery] string action)
        {
            if (string.IsNullOrEmpty(action)) return BadRequest("Команда (action) не вказана.");

            var allowedActions = new List<string> { "forward", "backward", "left", "right", "stop", "cam-up", "cam-down", "watchdog", "watchdog-stop", "lights-on", "lights-off" };
            if (!allowedActions.Contains(action.ToLower())) return BadRequest("Невідома команда руху.");

            if (!User.IsInRole("admin"))
            {
                var currentUserId = User.GetUserId();
                if (!(await robotService.CheckUserAccessToRobot(id, currentUserId)))
                {
                    return NotFound();
                }
            }

            try
            {
                await _hubContext.Clients.Group($"Robot_{id}").SendAsync("ReceiveCommand", action);
                return Ok(new { message = "Команду відправлено" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Помилка відправки: {ex.Message}");
            }
        }

        [AllowAnonymous]
        [HttpGet("status/{secretToken}")]
        public async Task<IActionResult> GetRobotMode(string secretToken)
        {
            var robot = await robotService.GetByToken(secretToken);
            if (robot == null) return NotFound("Робота не знайдено");

            return Ok(new { mode = robot.Status });
        }
    }
}