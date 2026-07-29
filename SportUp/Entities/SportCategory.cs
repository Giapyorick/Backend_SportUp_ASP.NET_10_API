using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SportUp.Entities
{
    public class SportCategory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty; // e.g., "Football", "Badminton"

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Status { get; set; } = string.Empty;

        // Navigation property for EF Core
        public ICollection<Match> Matches { get; set; } = new List<Match>();
    }
}
