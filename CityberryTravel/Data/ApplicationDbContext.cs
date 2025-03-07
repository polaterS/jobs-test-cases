using CityberryTravel.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

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
            
            modelBuilder.Entity<TravelDestination>()
                .Property(p => p.AvailableDates)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, new JsonSerializerOptions()),
                    v => JsonSerializer.Deserialize<List<DateTime>>(v, new JsonSerializerOptions()) ?? new List<DateTime>()
                );
        }
    }
}
