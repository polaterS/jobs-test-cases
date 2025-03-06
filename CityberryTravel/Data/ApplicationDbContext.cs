using CityberryTravel.Models;
using Microsoft.EntityFrameworkCore;

namespace CityberryTravel.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet properties
        public DbSet<TravelDestination> TravelDestinations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TravelDestination>().Property(p => p.Price).HasColumnType("decimal(18,2)");
        }
    }
}
