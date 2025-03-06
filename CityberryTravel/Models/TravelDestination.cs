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
        public string Name { get; set; }

        [StringLength(500)] 
        public string Description { get; set; }

        [Column(TypeName = "decimal(18, 2)")] 
        public decimal Price { get; set; }

        public List<DateTime> AvailableDates { get; set; }
    }
}
