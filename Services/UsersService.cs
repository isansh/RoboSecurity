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
                    UserRoles = user.UserRoles.Select(ur => ur.Role.RoleName).ToList(),
                })
                .ToList();
        }

        public UserResponse? GetByMail(string mail)
        {
            var user = dbContext.User
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
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
                UserRoles = user.UserRoles.Select(ur => ur.Role.RoleName).ToList(),
            };
        }

        public bool PostNew(string mail, string password, string confirmPassword, List<string> roles)
        {
            if (roles == null || !roles.Any())
            {
                return false;
            }

            var roleIds = dbContext.Role
                          .Where(r => roles.Contains(r.RoleName))
                          .Select(r => r.RoleId)
                          .ToList();

            if (roleIds.Count != roles.Count)
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

            var newUser = new UsersModel
            {
                UserMail = mail,
                UserPassword = hashPassword
            };

            dbContext.User.Add(newUser);

            foreach (var rId in roleIds)
            {
                dbContext.UserRoles.Add(new UserRolesModel
                {
                    UserId = newUser.UserId,
                    RoleId = rId
                });
            }

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
            var user = dbContext.User
                      .Include(u => u.UserRoles)
                      .FirstOrDefault(u => u.UserId == change.UserId);

            if (user == null)
            {
                return false;
            }

            if (change.UserRoles == null || !change.UserRoles.Any())
            {
                return false;
            }

            var newRoleIds = dbContext.Role
                             .Where(r => change.UserRoles.Contains(r.RoleName))
                             .Select(r => r.RoleId)
                             .ToList();

            if (newRoleIds.Count != change.UserRoles.Count)
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

            dbContext.UserRoles.RemoveRange(user.UserRoles);

            foreach (var rId in newRoleIds)
            {
                dbContext.UserRoles.Add(new UserRolesModel
                {
                    UserId = user.UserId,
                    RoleId = rId
                });
            }

            dbContext.SaveChanges();

            return true;
        }
    }
}