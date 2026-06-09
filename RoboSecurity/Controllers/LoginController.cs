using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using RoboSecurity.BLL.DTOs;
using RoboSecurity.BLL.Helpers;
using RoboSecurity.DAL.Models;
using RoboSecurity.BLL.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RoboSecurity.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LoginController : Controller
    {
        private readonly IUsersService userService;
        private readonly IConfiguration config;

        public LoginController(IUsersService userService, IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(userService);
            this.userService = userService;
            this.config = config;
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            var user = await userService.GetRawByMail(loginRequest.UserMail);

            if (user == null)
            {
                return Unauthorized("Користувача не знайдено");
            }

            var verifyPassword = HashCodeHelper.VerifyPassword(loginRequest.UserPassword, user.UserPassword);

            if (!verifyPassword)
            {
                return Unauthorized("Невірний пароль");
            }

            var userResponse = new UserResponse
            {
                UserId = user.UserId,
                UserMail = user.UserMail,
                PhoneNumber = user.PhoneNumber,
                UserRoles = user.UserRoles != null
            ? user.UserRoles.Select(ur => ur.Role?.RoleName ?? "Невідома роль").ToList()
            : new List<string>()
            };

            var token = GenerateJwtToken(userResponse);

            return Ok(new
            {
                Token = token
            });
        }

        private string GenerateJwtToken(UserResponse user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Email, user.UserMail),
            };

            if (user.UserRoles != null)
            {
                foreach (var role in user.UserRoles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }

            var token = new JwtSecurityToken(
                issuer: config["Jwt:Issuer"],
                audience: config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(120),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
