namespace TaskManagement.API.DTOs;

/// <summary>
/// DTO for creating a new task
/// </summary>
public class CreateTaskDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public int Priority { get; set; }
    public List<int> TagIds { get; set; } = new();
}

/// <summary>
/// DTO for updating an existing task
/// </summary>
public class UpdateTaskDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public int Priority { get; set; }
    public bool IsComplete { get; set; }
    public List<int> TagIds { get; set; } = new();
}

/// <summary>
/// DTO for task response
/// </summary>
public class TaskResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public int Priority { get; set; }
    public bool IsComplete { get; set; }
    public UserDetailsDto UserDetails { get; set; } = new();
    public List<TagDto> Tags { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO for user details
/// </summary>
public class UserDetailsDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Telephone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// DTO for tag
/// </summary>
public class TagDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// DTO for creating a tag
/// </summary>
public class CreateTagDto
{
    public string Name { get; set; } = string.Empty;
}
