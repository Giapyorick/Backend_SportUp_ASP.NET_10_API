using SportUp.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportUp.Entities;

public class RefreshToken
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    [Required]
    public string Token { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public bool IsRevoked { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Foreign Key constraint
    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }
}