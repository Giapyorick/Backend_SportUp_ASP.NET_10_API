using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SportUp.Entities
{
    public class Venue
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty; // e.g., "Dai Hoc Bach Khoa Stadium"

        [Required]
        [MaxLength(500)]
        public string Address { get; set; } = string.Empty;

        [MaxLength(200)]
        public string MapUrl { get; set; } = string.Empty; // Google Maps / Apple Maps link

        // Navigation property
        public ICollection<Match> Matches { get; set; } = new List<Match>();
    }
}
