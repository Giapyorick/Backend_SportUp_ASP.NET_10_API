using System.ComponentModel.DataAnnotations;

namespace SportUp.Entities
{
    public class Level
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty; // e.g., "Beginner", "Intermediate", "Advanced"

        [MaxLength(200)]
        public string Description { get; set; } = string.Empty;


    }
}
