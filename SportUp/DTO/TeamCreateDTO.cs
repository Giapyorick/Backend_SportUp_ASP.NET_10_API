using System.ComponentModel.DataAnnotations;

namespace SportUp.DTO
{
    public class TeamCreateDto
    {
        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty; // e.g., "Da Nang United"

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [MaxLength(100)]
        public string? Status { get; set; }
        public IFormFile? LogoUrl { get; set; }

        [Required]
        public IFormFile File { get; set; } = null!;
    }
}
