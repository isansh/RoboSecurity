using Microsoft.EntityFrameworkCore;

namespace RoboSecurity.Models
{
    public class DBContext : DbContext
    {
        public DBContext(DbContextOptions<DBContext> options)
        : base(options)
        {
        }

        public DbSet<UsersModel> User { get; set; }

        public DbSet<RolesModel> Role { get; set; }

        public DbSet<RobotsModel> Robot { get; set; }
    }
}
