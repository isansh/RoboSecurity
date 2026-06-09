using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoboSecurity.BLL.DTOs;
using RoboSecurity.DAL.Models;
using RoboSecurity.BLL.Services;
using RoboSecurity.BLL.Services.Interfaces;

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
            ArgumentNullException.ThrowIfNull(userService);
            this.userService = userService;
        }

        [Authorize(Roles = "admin")]
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await userService.GetAll();

            if (users is null)
                return NotFound("Користувачів не знайдено");

            return Ok(users);
        }

        [Authorize(Roles = "admin")]
        [HttpGet("{mail}")]
        public async Task<IActionResult> GetUserByMail(string mail)
        {
            var user = await userService.GetByMail(mail);

            if (user == null)
                return NotFound("Користувача не знайдено за мейлом");

            return Ok(user);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> PostUser([FromBody] CreateUserRequest request)
        {
            bool user = await userService.PostNew(request);

            if (!user)
                return NotFound("Не вдалося додати користувача");

            return Ok();
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserById(int id)
        {
            var user = await userService.DeleteById(id);

            if (!user)
                return NotFound("Користувача не знайдено за цим айді");

            return Ok();
        }

        [Authorize(Roles = "admin")]
        [HttpPut("edit")]
        public async Task<IActionResult> EditUser([FromBody] ChangeUserRequest change)
        {
            var result = await userService.EditUserDetails(change);

            if (!result)
            {
                return NotFound("Не вдалося змінити користувача");
            }

            return Ok();
        }
    }
}
