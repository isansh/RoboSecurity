using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoboSecurity.DTOs;
using RoboSecurity.Services;
using RoboSecurity.Services.Interfaces;

namespace RoboSecurity.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class RobotsController : Controller
    {
        private readonly IRobotsService robotService;
        private readonly IRobotConnectionService robotConnectionService;

        public RobotsController(IRobotsService robotService, IRobotConnectionService robotConnectionService)
        {
            this.robotService = robotService;
            this.robotConnectionService = robotConnectionService;
        }

        [Authorize]
        [HttpGet]
        public IActionResult GetRobots()
        {
            var robo = robotService.GetAll();

            if (robo is null)
                return NotFound();

            return Ok(robo);
        }

        [Authorize]
        [HttpGet("id/{id}")]
        public IActionResult GetRobotById(int id)
        {
            var robo = robotService.GetById(id);

            if (robo == null)
                return NotFound();

            return Ok(robo);
        }

        [Authorize]
        [HttpGet("name/{name}")]
        public IActionResult GetRobotByName(string name)
        {
            var robo = robotService.GetByName(name);

            if (robo == null)
                return NotFound();

            return Ok(robo);
        }

        [Authorize]
        [HttpGet("user/{userId}")]
        public IActionResult GetRobotsByUserId(int userId)
        {
            var robots = robotService.GetByUserId(userId);

            if (robots == null || !robots.Any())
                return NotFound();

            return Ok(robots);
        }

        [Authorize]
        [HttpPost]
        public IActionResult PostRobot([FromBody] CreateRobotRequest request)
        {
            bool robo = robotService.PostNew(request);

            if (!robo)
                return NotFound();

            return Ok();
        }

        [Authorize]
        [HttpDelete("{id}")]
        public IActionResult DeleteRobotById(int id)
        {
            var robo = robotService.DeleteById(id);

            if (!robo)
                return NotFound();

            return Ok();
        }

        [Authorize]
        [HttpPut("edit")]
        public IActionResult EditRobot([FromBody] RobotResponse change)
        {
            var result = robotService.EditRobotsDetails(change);

            if (!result)
            {
                return NotFound();
            }

            return Ok();
        }

        [Authorize]
        [HttpPut("activate")]
        public IActionResult ActivateRobot([FromBody] RobotActivationDto request)
        {
            var result = robotService.ActivateRobot(request);

            if (!result)
            {
                return NotFound();
            }

            return Ok();
        }

        [AllowAnonymous]
        [HttpPost("ping")]
        public IActionResult RobotPing([FromBody] RobotActivationDto request)
        {
            if (request == null || string.IsNullOrEmpty(request.SecretToken))
                return BadRequest("Токен відсутній.");

            var robot = robotService.GetByToken(request.SecretToken);
            if (robot == null)
                return Unauthorized("Невірний токен робота.");

            var currentRobotIp = HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();

            if (string.IsNullOrEmpty(currentRobotIp))
                return BadRequest("Не вдалося визначити IP-адресу пристрою.");

            robotConnectionService.RegisterRobotIp(robot.RoboId, currentRobotIp);

            return Ok(new { status = "registered" });
        }

        [Authorize]
        [HttpPost("id/{id}/control")]
        public async Task<IActionResult> SendCommand(int id, [FromQuery] string action)
        {
            if (string.IsNullOrEmpty(action))
            {
                return BadRequest("Команда (action) не вказана.");
            }

            var allowedActions = new List<string> { "forward", "backward", "left", "right", "stop", "cam-up", "cam-down", "watchdog", "watchdog-stop" };
            if (!allowedActions.Contains(action.ToLower()))
            {
                return BadRequest("Невідома команда руху.");
            }

            bool isSuccess = await robotConnectionService.SendControlCommandAsync(id, action);

            if (isSuccess)
            {
                return Ok(new { message = $"Команда {action} успішно виконана роботом." });
            }

            return StatusCode(504, "Робот не відповів на команду або знаходиться в офлайні.");
        }

        [AllowAnonymous]
        [HttpGet("id/{id}/video")]
        public async Task<IActionResult> GetVideoFeed(int id)
        {

            var videoStream = await robotConnectionService.GetRobotVideoReaderAsync(id);

            if (videoStream == null)
            {
                return NotFound("Відеопотік недоступний або робота не знайдено");
            }

            return File(videoStream, "multipart/x-mixed-replace; boundary=frame");
        }

        [AllowAnonymous]
        [HttpGet("status/{secretToken}")]
        public IActionResult GetRobotMode(string secretToken)
        {
            var robot = robotService.GetByToken(secretToken);

            if (robot == null)
            {
                return NotFound("Робота не знайдено");
            }

            return Ok(new { mode = robot.Status });
        }
    }
}
