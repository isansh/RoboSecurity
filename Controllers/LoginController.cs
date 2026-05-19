using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using RoboSecurity.DTOs;
using RoboSecurity.Helpers;
using RoboSecurity.Models;
using RoboSecurity.Services.Interfaces;
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
            this.userService = userService;
            this.config = config;
        }

        [HttpPost]
        public IActionResult Login([FromBody] LoginRequest loginRequest)
        {
            var user = userService.GetByMail(loginRequest.UserMail);

            if (user == null)
            {
                return Unauthorized("Користувача не знайдено");
            }

            var verifyPassword = HashCodeHelper.VerifyPassword(loginRequest.UserPassword, user.UserPassword);

            if (!verifyPassword)
            {
                return Unauthorized("Невірний пароль");
            }

            var token = GenerateJwtToken(user);

            return Ok(new
            {
                Token = token,
                UserRoles = user.UserRoles
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
