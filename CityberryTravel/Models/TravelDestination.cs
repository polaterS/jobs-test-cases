using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CityberryTravel.Models
{
    public class TravelDestination
    {
        [Key] 
        public int Id { get; set; }

        [Required] 
        [StringLength(100)] 
        public required string Name { get; set; }

        [StringLength(500)] 
        public required string Description { get; set; }

        [Column(TypeName = "decimal(18, 2)")] 
        public required decimal Price { get; set; }

        [Display(Name = "Available Dates & Times")]
        public List<DateTime> AvailableDates { get; set; } = new List<DateTime>();
    }
}
