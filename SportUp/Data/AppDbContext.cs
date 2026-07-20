using Microsoft.EntityFrameworkCore;
using SportUp.Entities;

namespace SportUp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<SportCategory> SportCategories { get; set; } = null!;
        public DbSet<Level> Levels { get; set; } = null!;
        public DbSet<Venue> Venues { get; set; } = null!;
        public DbSet<Team> Teams { get; set; } = null!;
        public DbSet<Match> Matches { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Add any Fluent API configuration here if needed in future
        }
    }
}
