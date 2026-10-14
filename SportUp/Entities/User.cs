using System.ComponentModel.DataAnnotations;

namespace SportUp.Entities;

public class User
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? FullName { get; set; }

    [Required]
    [MaxLength(20)]
    public string Role { get; set; } = "User"; // "Admin", "Manager", "User"

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Relationship: One User has many RefreshTokens
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}