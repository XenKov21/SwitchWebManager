using Microsoft.EntityFrameworkCore;
using SwitchWebManager.Models;
using System.Collections.Generic;

namespace SwitchWebManager.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users => Set<User>();

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    }
}
