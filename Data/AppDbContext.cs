using Microsoft.EntityFrameworkCore;
using AmrTools.Models;
using BCrypt.Net;

namespace AmrTools.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Event> Events { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var adminId = Guid.Parse("7470b363-c36e-4111-b429-07972cf7a1b9");

            string hashedPw = BCrypt.Net.BCrypt.HashPassword("admin123");

            modelBuilder.Entity<User>().HasData(new User
            {
                Id = adminId,
                FullName = "Super Admin",
                Email = "admin@amrtools.com",
                PasswordHash = hashedPw, 
                Role = "Admin",
                SubscriptionPlan = "Pro"
            });

        }
    }
}

