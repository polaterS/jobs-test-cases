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
        public string Name { get; set; } = string.Empty;

        [StringLength(500)] 
        public string Description { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18, 2)")] 
        public decimal Price { get; set; }

        [Display(Name = "Available Dates & Times")]
        public List<DateTime> AvailableDates { get; set; } = new List<DateTime>();
    }
}
