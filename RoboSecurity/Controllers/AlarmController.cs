using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoboSecurity.BLL.DTOs;
using RoboSecurity.BLL.Services;
using RoboSecurity.BLL.Services.Interfaces;

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
            ArgumentNullException.ThrowIfNull(alarmsService);
            this.alarmsService = alarmsService;
        }

        [Authorize(Roles = "admin, guard")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await alarmsService.GetAllAlarms());
        }

        [Authorize(Roles = "admin, guard")]
        [HttpGet("active")]
        public async Task<IActionResult> GetActive()
        {
            return Ok(await alarmsService.GetUnresolvedAlarms());
        }

        [HttpPost("upload")]
        [AllowAnonymous]
        public async Task<IActionResult> Create([FromBody] AlarmRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.ImageBase64))
                return BadRequest("Некоректні дані запиту або відсутній Base64-код картинки");

            var result = await alarmsService.CreateAlarm(request);

            if (!result) return BadRequest("Не вдалося створити тривогу (робот не знайдений або помилка запису)");

            return Ok();
        }

        [Authorize(Roles = "admin, guard")]
        [HttpPut("resolve/{id}")]
        public async Task<IActionResult> Resolve(int id)
        {
            var result = await alarmsService.ResolveAlarm(id);
            if (!result)
            {
                return NotFound("Аларм з таким ID не знайдено"); 
            }

            return Ok("Тривогу успішно закрито");
        }

        [Authorize(Roles = "admin, guard")]
        [HttpGet("search")]
        public async Task<IActionResult> GetByDate([FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            if (from == DateTime.MinValue || to == DateTime.MinValue)
            {
                return BadRequest("Некоректні дати");
            }

            if (from > to)
            {
                return BadRequest("Дата початку періоду не може бути більшою за дату кінця");
            }

            var alarms = await alarmsService.GetAlarmsByDate(from, to);
            return Ok(alarms);
        }
    }
}
