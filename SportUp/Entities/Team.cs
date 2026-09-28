using System;
using System.ComponentModel.DataAnnotations;

namespace SportUp.Entities
{
    public class Team
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty; // e.g., "Da Nang United"

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(255)]
        public string LogoUrl { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        public string? Status { get; set; }

        // Note: In Sprint 3, you'll add the relationships for members/rosters here
    }
}
