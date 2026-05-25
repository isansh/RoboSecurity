using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoboSecurity.DTOs;
using RoboSecurity.Models;
using RoboSecurity.Services;
using RoboSecurity.Services.Interfaces;

namespace RoboSecurity.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUsersService userService;

        public UsersController(IUsersService userService)
        {
            this.userService = userService;
        }

        [Authorize(Roles = "admin")]
        [HttpGet]
        public IActionResult GetUsers()
        {
            var users = userService.GetAll();

            if (users is null)
                return NotFound("Користувачів не знайдено");

            return Ok(users);
        }

        [Authorize(Roles = "admin")]
        [HttpGet("{mail}")]
        public IActionResult GetUserByMail(string mail)
        {
            var user = userService.GetByMail(mail);

            if (user == null)
                return NotFound("Користувача не знайдено за мейлом");

            return Ok(user);
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult PostUser([FromBody] CreateUserRequest request)
        {
            bool user = userService.PostNew(request);

            if (!user)
                return NotFound("Не вдалося додати користувача");

            return Ok();
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public IActionResult DeleteUserById(int id)
        {
            var user = userService.DeleteById(id);

            if (!user)
                return NotFound("Користувача не знайдено за цим айді");

            return Ok();
        }

        [Authorize(Roles = "admin")]
        [HttpPut("edit")]
        public IActionResult EditUser([FromBody] ChangeUserRequest change)
        {
            var result = userService.EditUserDetails(change);

            if (!result)
            {
                return NotFound("Не вдалося змінити користувача");
            }

            return Ok();
        }
    }
}
