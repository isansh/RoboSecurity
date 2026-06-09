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
        public async Task<IActionResult> Create([FromForm] AlarmRequest request, IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("Файл відсутній");

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "alarms");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativeUrlPath = $"/uploads/alarms/{uniqueFileName}";

            var result = await alarmsService.CreateAlarm(request, relativeUrlPath);

            if (!result) return BadRequest("Не вдалося створити тривогу");
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
