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
                    PhoneNumber = user.PhoneNumber,
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
                PhoneNumber = user.PhoneNumber,
                UserPassword = user.UserPassword,
                UserRoles = user.UserRoles.Select(ur => ur.Role.RoleName).ToList(),
            };
        }

        public bool PostNew(CreateUserRequest create)
        {
            if (create.UserRoles == null || !create.UserRoles.Any())
            {
                return false;
            }

            var roleIds = dbContext.Role
                          .Where(r => create.UserRoles.Contains(r.RoleName))
                          .Select(r => r.RoleId)
                          .ToList();

            if (roleIds.Count != create.UserRoles.Count)
            {
                return false;
            }

            if (!VerifyingRequestHelper.VerifyQuery(create.UserMail) ||
                !VerifyingRequestHelper.VerifyQuery(create.PhoneNumber) ||
                !VerifyingRequestHelper.VerifyQuery(create.Password) || 
                !VerifyingRequestHelper.VerifyQuery(create.ConfirmPassword))
            {
                return false;
            }

            var exists = dbContext.User.Any(u => u.UserMail == create.UserMail);

            if (exists)
            {
                return false;
            }

            if (create.Password != create.ConfirmPassword)
            {
                return false;
            }

            var hashPassword = HashCodeHelper.HashPassword(create.Password);

            if (!VerifyingRequestHelper.VerifyQuery(hashPassword))
            {
                return false;
            }

            var newUser = new UsersModel
            {
                UserMail = create.UserMail,
                PhoneNumber = create.Password,
                UserPassword = hashPassword
            };

            dbContext.User.Add(newUser);
            dbContext.SaveChanges();

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
                !VerifyingRequestHelper.VerifyQuery(change.PhoneNumber) ||
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
            user.PhoneNumber = change.PhoneNumber;
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