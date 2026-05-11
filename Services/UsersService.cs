using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RoboSecurity.DTOs;
using RoboSecurity.Helpers;
using RoboSecurity.Models;
using RoboSecurity.Services.Interfaces;
using System.Data;

namespace RoboSecurity.Services
{
    public class UsersService : IUsersService
    {
        private readonly DBContext dbContext;

        public UsersService(DBContext db)
        {
            dbContext = db;
        }

        public List<UserResponse> GetAll()
        {
            return dbContext.User
                .Select(user => new UserResponse
                {
                    UserId = user.UserId,
                    UserMail = user.UserMail,
                    UserPassword = user.UserPassword,
                    UserRoleName = user.Roles.RoleName,
                })
                .ToList();
        }

        public UserResponse? GetByMail(string mail)
        {
            var user = dbContext.User
                .Include(u => u.Roles)
                .FirstOrDefault(u => u.UserMail == mail);

            if (user == null)
            {
                return null;
            }

            return new UserResponse
            {
                UserId = user.UserId,
                UserMail = user.UserMail,
                UserPassword = user.UserPassword,
                UserRoleName = user.Roles.RoleName,
            };
        }

        public bool PostNew(string mail, string password, string confirmPassword, string role)
        {
            var roleId = dbContext.Role
                .Where(r => r.RoleName == role)
                .Select(r => r.RoleId)
                .FirstOrDefault();

            if (roleId == 0)
            {
                return false;
            }

            if (!VerifyingRequestHelper.VerifyQuery(mail) || 
                !VerifyingRequestHelper.VerifyQuery(password) || 
                !VerifyingRequestHelper.VerifyQuery(confirmPassword))
            {
                return false;
            }

            var exists = dbContext.User.Any(u => u.UserMail == mail);

            if (exists)
            {
                return false;
            }

            if (password != confirmPassword)
            {
                return false;
            }

            var hashPassword = HashCodeHelper.HashPassword(password);

            if (!VerifyingRequestHelper.VerifyQuery(hashPassword))
            {
                return false;
            }

            dbContext.User.Add(new UsersModel
            {
                UserMail = mail,
                UserPassword = hashPassword,
                UserRoleId = roleId,
            });

            dbContext.SaveChanges();

            return true;
        }

        public bool DeleteById(int id)
        {
            var user = dbContext.User.Find(id);

            if (user == null)
            {
                return false;
            }

            dbContext.User.Remove(user);

            dbContext.SaveChanges();

            return true;
        }

        public bool EditUserDetails(ChangeUserRequest change)
        {
            var user = dbContext.User.Find(change.UserId);

            if (user == null)
            {
                return false;
            }

            if (!dbContext.Role.Any(r => r.RoleId == change.UserRoleId))
            {
                return false;
            }

            if (!VerifyingRequestHelper.VerifyQuery(change.UserMail) ||
                !VerifyingRequestHelper.VerifyQuery(change.Password) ||
                !VerifyingRequestHelper.VerifyQuery(change.ConfirmPassword))
            {
                return false;
            }

            if (change.Password != change.ConfirmPassword)
            {
                return false;
            }

            var exists = dbContext.User.Any(u => u.UserMail == change.UserMail && u.UserId != change.UserId);

            if (exists)
            {
                return false;
            }

            var hashPassword = HashCodeHelper.HashPassword(change.Password);

            user.UserMail = change.UserMail;
            user.UserPassword = hashPassword;
            user.UserRoleId = change.UserRoleId;

            dbContext.SaveChanges();

            return true;
        }
    }
}