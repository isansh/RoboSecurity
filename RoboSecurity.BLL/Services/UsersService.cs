using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RoboSecurity.BLL.DTOs;
using RoboSecurity.BLL.Helpers;
using RoboSecurity.DAL.Models;
using RoboSecurity.BLL.Services.Interfaces;

namespace RoboSecurity.BLL.Services
{
    public class UsersService : IUsersService
    {
        private readonly DBContext dbContext;

        public UsersService(DBContext db)
        {
            dbContext = db;
        }

        public async Task<List<UserResponse>> GetAll()
        {
            var users = await dbContext.User
                 .Include(u => u.UserRoles)
                 .ThenInclude(ur => ur.Role)
                 .ToListAsync();

            return users.Select(u => MapToUserResponse(u)).ToList();
        }

        public async Task<UserResponse?> GetByMail(string mail)
        {
            var user = await dbContext.User
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.UserMail == mail);

            if (user == null) return null;

            return MapToUserResponse(user);
        }

        public async Task<UserResponse?> GetById(int id)
        {
            var user = await dbContext.User
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null) return null;

            return MapToUserResponse(user);
        }

        public async Task<bool> PostNew(CreateUserRequest create)
        {
            if (create.UserRoles == null || !create.UserRoles.Any()) return false;

            var roleIds = await dbContext.Role
                          .Where(r => create.UserRoles.Contains(r.RoleName))
                          .Select(r => r.RoleId)
                          .ToListAsync();

            if (roleIds.Count != create.UserRoles.Count) return false;

            if (!VerifyingRequestHelper.VerifyQuery(create.UserMail) ||
                !VerifyingRequestHelper.VerifyQuery(create.PhoneNumber) ||
                !VerifyingRequestHelper.VerifyQuery(create.Password) ||
                !VerifyingRequestHelper.VerifyQuery(create.ConfirmPassword))
            {
                return false;
            }

            var exists = await dbContext.User.AnyAsync(u => u.UserMail == create.UserMail);
            if (exists) return false;

            if (create.Password != create.ConfirmPassword) return false;

            var hashPassword = HashCodeHelper.HashPassword(create.Password);
            if (!VerifyingRequestHelper.VerifyQuery(hashPassword)) return false;

            var newUser = new UsersModel
            {
                UserMail = create.UserMail,
                PhoneNumber = create.PhoneNumber,
                UserPassword = hashPassword
            };

            dbContext.User.Add(newUser);
            await dbContext.SaveChangesAsync();

            foreach (var rId in roleIds)
            {
                dbContext.UserRoles.Add(new UserRolesModel
                {
                    UserId = newUser.UserId,
                    RoleId = rId
                });
            }

            await dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteById(int id)
        {
            var user = await dbContext.User.FindAsync(id);
            if (user == null) return false;

            dbContext.User.Remove(user);
            await dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EditUserDetails(ChangeUserRequest change)
        {
            var user = await dbContext.User
                      .Include(u => u.UserRoles)
                      .FirstOrDefaultAsync(u => u.UserId == change.UserId);

            if (user == null) return false;

            if (!VerifyingRequestHelper.VerifyQuery(change.UserMail) ||
                !VerifyingRequestHelper.VerifyQuery(change.PhoneNumber))
            {
                return false;
            }

            var exists = await dbContext.User.AnyAsync(u => u.UserMail == change.UserMail && u.UserId != change.UserId);
            if (exists) return false;

            if (!string.IsNullOrEmpty(change.Password) || !string.IsNullOrEmpty(change.ConfirmPassword))
            {
                if (change.Password != change.ConfirmPassword) return false;
                if (!VerifyingRequestHelper.VerifyQuery(change.Password)) return false;

                var hashPassword = HashCodeHelper.HashPassword(change.Password);
                user.UserPassword = hashPassword;
            }

            user.UserMail = change.UserMail;
            user.PhoneNumber = change.PhoneNumber;

            if (change.UserRoles != null && change.UserRoles.Any())
            {
                var newRoleIds = await dbContext.Role
                    .Where(r => change.UserRoles.Contains(r.RoleName))
                    .Select(r => r.RoleId)
                    .ToListAsync();

                if (newRoleIds.Count == change.UserRoles.Count)
                {
                    dbContext.UserRoles.RemoveRange(user.UserRoles);

                    foreach (var rId in newRoleIds)
                    {
                        dbContext.UserRoles.Add(new UserRolesModel
                        {
                            UserId = user.UserId,
                            RoleId = rId
                        });
                    }
                }
            }

            await dbContext.SaveChangesAsync();
            return true;
        }

        private static UserResponse MapToUserResponse(UsersModel user)
        {
            return new UserResponse
            {
                UserId = user.UserId,
                UserMail = user.UserMail,
                PhoneNumber = user.PhoneNumber,
                UserRoles = user.UserRoles != null
                    ? user.UserRoles.Select(ur => ur.Role?.RoleName ?? "Невідома роль").ToList()
                    : new List<string>()
            };
        }

        public async Task<UsersModel?> GetRawByMail(string mail)
        {
            return await dbContext.User
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.UserMail == mail);
        }
    }
}