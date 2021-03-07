using Microsoft.EntityFrameworkCore;

namespace UserAPI.Models
{
    public class UserAPIContext : DbContext
    {
        public UserAPIContext(DbContextOptions<UserAPIContext> options) : base(options)
        {

        }

        public DbSet<UserModel> Users { get; set; }
    }
}
