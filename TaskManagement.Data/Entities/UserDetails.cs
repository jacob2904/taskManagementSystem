using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Data.Entities;

/// <summary>
/// Represents user details for task assignment and authentication
/// </summary>
public class UserDetails
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Full name is required")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 200 characters")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Telephone is required")]
    [Phone(ErrorMessage = "Invalid telephone number format")]
    [StringLength(20, ErrorMessage = "Telephone cannot exceed 20 characters")]
    public string Telephone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address format")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password hash is required")]
    [StringLength(500)]
    public string PasswordHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastLoginAt { get; set; }

    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
