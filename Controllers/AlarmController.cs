using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoboSecurity.DTOs;
using RoboSecurity.Services.Interfaces;

namespace RoboSecurity.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class AlarmsController : ControllerBase
    {
        private readonly IAlarmsService alarmsService;

        public AlarmsController(IAlarmsService alarmsService)
        {
            this.alarmsService = alarmsService;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(alarmsService.GetAllAlarms());
        }

        [HttpGet("active")]
        public IActionResult GetActive()
        {
            return Ok(alarmsService.GetUnresolvedAlarms());
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult Create([FromBody] AlarmRequest request)
        {
            var result = alarmsService.CreateAlarm(request);
            if (!result)
            {
                return BadRequest("Не вдалося створити сповіщення");
            }

            return Ok("Тривогу зареєстровано");
        }

        [HttpPut("resolve/{id}")]
        public IActionResult Resolve(int id)
        {
            var result = alarmsService.ResolveAlarm(id);
            if (!result)
            {
                return NotFound("Аларм з таким ID не знайдено"); 
            }

            return Ok("Тривогу успішно закрито");
        }

        [HttpGet("search")]
        public IActionResult GetByDate([FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            if (from == DateTime.MinValue || to == DateTime.MinValue)
            {
                return BadRequest("Некоректні дати");
            }

            var alarms = alarmsService.GetAlarmsByDate(from, to);
            return Ok(alarms);
        }
    }
}
