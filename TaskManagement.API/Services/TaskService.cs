using Microsoft.EntityFrameworkCore;
using TaskManagement.API.DTOs;
using TaskManagement.API.Services.Interfaces;
using TaskManagement.Data;
using TaskManagement.Data.Entities;
using static TaskManagement.API.Services.TimezoneService;

namespace TaskManagement.API.Services;

/// <summary>
/// Service for task operations with user isolation
/// </summary>
public class TaskService : ITaskService
{
    private readonly TaskManagementDbContext _context;
    private readonly ILogger<TaskService> _logger;

    public TaskService(TaskManagementDbContext context, ILogger<TaskService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all tasks for a user
    /// </summary>
    public async Task<IEnumerable<TaskResponseDto>> GetAllTasksAsync(int userId)
    {
        try
        {
            var tasks = await _context.Tasks
                .Where(t => t.UserDetailsId == userId)
                .Include(t => t.UserDetails)
                .Include(t => t.Tags)
                .Select(t => new TaskResponseDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    DueDate = t.DueDate,
                    Priority = t.Priority,
                    IsComplete = t.IsComplete,
                    UserDetails = new UserDetailsDto
                    {
                        Id = t.UserDetails!.Id,
                        FullName = t.UserDetails.FullName,
                        Telephone = t.UserDetails.Telephone,
                        Email = t.UserDetails.Email
                    },
                    Tags = t.Tags.Select(tag => new TagDto
                    {
                        Id = tag.Id,
                        Name = tag.Name
                    }).ToList(),
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt
                })
                .ToListAsync();

            return tasks;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tasks for user {UserId}", userId);
            throw new InvalidOperationException("An error occurred while retrieving tasks", ex);
        }
    }

    /// <summary>
    /// Get task by ID for a user
    /// </summary>
    public async Task<TaskResponseDto?> GetTaskByIdAsync(int id, int userId)
    {
        try
        {
            var task = await _context.Tasks
                .Where(t => t.Id == id && t.UserDetailsId == userId)
                .Include(t => t.UserDetails)
                .Include(t => t.Tags)
                .Select(t => new TaskResponseDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    DueDate = t.DueDate,
                    Priority = t.Priority,
                    IsComplete = t.IsComplete,
                    UserDetails = new UserDetailsDto
                    {
                        Id = t.UserDetails!.Id,
                        FullName = t.UserDetails.FullName,
                        Telephone = t.UserDetails.Telephone,
                        Email = t.UserDetails.Email
                    },
                    Tags = t.Tags.Select(tag => new TagDto
                    {
                        Id = tag.Id,
                        Name = tag.Name
                    }).ToList(),
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt
                })
                .FirstOrDefaultAsync();

            return task;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving task {TaskId} for user {UserId}", id, userId);
            throw new InvalidOperationException($"An error occurred while retrieving task {id}", ex);
        }
    }

    /// <summary>
    /// Create a new task for a user
    /// </summary>
    public async Task<TaskResponseDto> CreateTaskAsync(CreateTaskDto createTaskDto, int userId)
    {
        try
        {
            var userDetails = await _context.UserDetails.FindAsync(userId);
            if (userDetails == null)
            {
                throw new InvalidOperationException("User not found");
            }

            var tags = await ValidateAndGetTagsAsync(createTaskDto.TagIds, userId);

            var task = new TaskItem
            {
                Title = createTaskDto.Title,
                Description = createTaskDto.Description,
                DueDate = createTaskDto.DueDate.ToUniversalTime(),
                Priority = createTaskDto.Priority,
                UserDetailsId = userId,
                Tags = tags,
                CreatedAt = DateTime.UtcNow
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            return new TaskResponseDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                DueDate = task.DueDate,
                Priority = task.Priority,
                IsComplete = task.IsComplete,
                UserDetails = new UserDetailsDto
                {
                    Id = userDetails.Id,
                    FullName = userDetails.FullName,
                    Telephone = userDetails.Telephone,
                    Email = userDetails.Email
                },
                Tags = tags.Select(t => new TagDto { Id = t.Id, Name = t.Name }).ToList(),
                CreatedAt = task.CreatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating task for user {UserId}", userId);
            throw new InvalidOperationException("An error occurred while creating the task", ex);
        }
    }

    /// <summary>
    /// Update an existing task for a user
    /// </summary>
    public async Task<TaskResponseDto?> UpdateTaskAsync(int id, UpdateTaskDto updateTaskDto, int userId)
    {
        try
        {
            var task = await _context.Tasks
                .Where(t => t.Id == id && t.UserDetailsId == userId)
                .Include(t => t.Tags)
                .FirstOrDefaultAsync();

            if (task == null)
            {
                return null;
            }

            var userDetails = await _context.UserDetails.FindAsync(userId);
            if (userDetails == null)
            {
                throw new InvalidOperationException("User not found");
            }

            var tags = await ValidateAndGetTagsAsync(updateTaskDto.TagIds, userId);

            task.Title = updateTaskDto.Title;
            task.Description = updateTaskDto.Description;
            task.DueDate = updateTaskDto.DueDate.ToUniversalTime();
            task.Priority = updateTaskDto.Priority;
            task.IsComplete = updateTaskDto.IsComplete;
            task.Tags = tags;
            task.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new TaskResponseDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                DueDate = task.DueDate,
                Priority = task.Priority,
                IsComplete = task.IsComplete,
                UserDetails = new UserDetailsDto
                {
                    Id = userDetails.Id,
                    FullName = userDetails.FullName,
                    Telephone = userDetails.Telephone,
                    Email = userDetails.Email
                },
                Tags = tags.Select(t => new TagDto { Id = t.Id, Name = t.Name }).ToList(),
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task {TaskId} for user {UserId}", id, userId);
            throw new InvalidOperationException($"An error occurred while updating task {id}", ex);
        }
    }

    /// <summary>
    /// Mark a task as complete for a user
    /// </summary>
    public async Task<TaskResponseDto?> MarkTaskCompleteAsync(int id, int userId)
    {
        try
        {
            var task = await _context.Tasks
                .Where(t => t.Id == id && t.UserDetailsId == userId)
                .Include(t => t.UserDetails)
                .Include(t => t.Tags)
                .FirstOrDefaultAsync();

            if (task == null)
            {
                return null;
            }

            task.IsComplete = true;
            task.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return new TaskResponseDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                DueDate = task.DueDate,
                Priority = task.Priority,
                IsComplete = task.IsComplete,
                UserDetails = new UserDetailsDto
                {
                    Id = task.UserDetails!.Id,
                    FullName = task.UserDetails.FullName,
                    Telephone = task.UserDetails.Telephone,
                    Email = task.UserDetails.Email
                },
                Tags = task.Tags.Select(t => new TagDto { Id = t.Id, Name = t.Name }).ToList(),
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking task {TaskId} as complete for user {UserId}", id, userId);
            throw new InvalidOperationException($"An error occurred while marking task {id} as complete", ex);
        }
    }

    /// <summary>
    /// Get all incomplete overdue tasks for a user
    /// </summary>
    public async Task<IEnumerable<TaskResponseDto>> GetOverdueTasksAsync(int userId)
    {
        try
        {
            var now = DateTime.UtcNow;
            var overdueTasks = await _context.Tasks
                .Where(t => t.UserDetailsId == userId && !t.IsComplete && t.DueDate <= now)
                .Include(t => t.UserDetails)
                .Include(t => t.Tags)
                .Select(t => new TaskResponseDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    DueDate = t.DueDate,
                    Priority = t.Priority,
                    IsComplete = t.IsComplete,
                    UserDetails = new UserDetailsDto
                    {
                        Id = t.UserDetails!.Id,
                        FullName = t.UserDetails.FullName,
                        Telephone = t.UserDetails.Telephone,
                        Email = t.UserDetails.Email
                    },
                    Tags = t.Tags.Select(tag => new TagDto
                    {
                        Id = tag.Id,
                        Name = tag.Name
                    }).ToList(),
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt
                })
                .ToListAsync();

            return overdueTasks;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving overdue tasks for user {UserId}", userId);
            throw new InvalidOperationException("An error occurred while retrieving overdue tasks", ex);
        }
    }

    /// <summary>
    /// Delete a task for a user
    /// </summary>
    public async Task<bool> DeleteTaskAsync(int id, int userId)
    {
        try
        {
            var task = await _context.Tasks
                .FirstOrDefaultAsync(t => t.Id == id && t.UserDetailsId == userId);

            if (task == null)
            {
                return false;
            }

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting task {TaskId} for user {UserId}", id, userId);
            throw new InvalidOperationException($"An error occurred while deleting task {id}", ex);
        }
    }

    /// <summary>
    /// Validate and get tags by IDs for a user
    /// </summary>
    private async Task<List<Tag>> ValidateAndGetTagsAsync(List<int> tagIds, int userId)
    {
        if (!tagIds.Any())
        {
            return new List<Tag>();
        }

        var tags = await _context.Tags
            .Where(t => tagIds.Contains(t.Id) && t.UserDetailsId == userId)
            .ToListAsync();

        if (tags.Count != tagIds.Count)
        {
            throw new InvalidOperationException("One or more tag IDs are invalid or do not belong to the user");
        }

        return tags;
    }
}
