using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagement.Data.Entities;

/// <summary>
/// Represents a tag that can be associated with tasks
/// </summary>
public class Tag
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Tag name is required")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Tag name must be between 2 and 50 characters")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "User details are required")]
    public int UserDetailsId { get; set; }

    [ForeignKey(nameof(UserDetailsId))]
    public UserDetails? UserDetails { get; set; }

    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
