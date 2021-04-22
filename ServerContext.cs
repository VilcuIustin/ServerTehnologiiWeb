using Microsoft.EntityFrameworkCore;
using Server_Tehnologii_Web.Models;

namespace Server_Tehnologii_Web
{
    public class ServerContext : DbContext
    {
        public ServerContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Group> Groups { get; set; }
    }
}