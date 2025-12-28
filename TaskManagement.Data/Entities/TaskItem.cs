using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagement.Data.Entities;

/// <summary>
/// Represents a task item with user details and tags
/// </summary>
public class TaskItem
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 2000 characters")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Due date is required")]
    public DateTime DueDate { get; set; }

    [Required(ErrorMessage = "Priority is required")]
    [Range(1, 5, ErrorMessage = "Priority must be between 1 and 5")]
    public int Priority { get; set; }

    public bool IsComplete { get; set; } = false;

    [Required(ErrorMessage = "User details are required")]
    public int UserDetailsId { get; set; }

    [ForeignKey(nameof(UserDetailsId))]
    public UserDetails? UserDetails { get; set; }

    public ICollection<Tag> Tags { get; set; } = new List<Tag>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
