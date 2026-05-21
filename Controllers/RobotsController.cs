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

        public RobotsController(IRobotsService robotService)
        {
            this.robotService = robotService;
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
            bool robo = robotService.PostNew(request.RoboName, request.RoboIpAdress, request.UserId, request.StreamUrl, request.Status);

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
    }
}
