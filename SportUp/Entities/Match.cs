using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportUp.Entities
{
    public class Match
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty; // e.g., "Friendly 5v5 Match - Looking for Midfielders"

        [Required]
        public int SportCategoryId { get; set; }
        [ForeignKey(nameof(SportCategoryId))]
        public SportCategory? SportCategory { get; set; }

        [Required]
        public int VenueId { get; set; }
        [ForeignKey(nameof(VenueId))]
        public Venue? Venue { get; set; }

        [Required]
        public int TargetLevelId { get; set; }
        [ForeignKey(nameof(TargetLevelId))]
        public Level? TargetLevel { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        [Required]
        public int TotalSlots { get; set; } // Maximum number of players allowed

        public int AvailableSlots { get; set; } // Decrements when players are accepted

        [Column(TypeName = "decimal(18,2)")]
        public decimal PricePerSlot { get; set; } // Projected cost split per person

        [MaxLength(1000)]
        public string Note { get; set; } = string.Empty; // Special requests (e.g., "Bring white shirts")

        // Temporary Mocking: Since Auth is in Sprint 2, we track who created it via a string or integer
        [Required]
        public string CreatedByUserId { get; set; } = "SYSTEM_MOCK_USER"; 

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
