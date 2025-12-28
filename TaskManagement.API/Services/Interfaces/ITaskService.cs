using TaskManagement.API.DTOs;

namespace TaskManagement.API.Services.Interfaces;

/// <summary>
/// Service interface for task operations
/// </summary>
public interface ITaskService
{
    /// <summary>
    /// Get all tasks for a user
    /// </summary>
    Task<IEnumerable<TaskResponseDto>> GetAllTasksAsync(int userId);

    /// <summary>
    /// Get task by ID for a user
    /// </summary>
    Task<TaskResponseDto?> GetTaskByIdAsync(int id, int userId);

    /// <summary>
    /// Create a new task for a user
    /// </summary>
    Task<TaskResponseDto> CreateTaskAsync(CreateTaskDto createTaskDto, int userId);

    /// <summary>
    /// Update an existing task for a user
    /// </summary>
    Task<TaskResponseDto?> UpdateTaskAsync(int id, UpdateTaskDto updateTaskDto, int userId);

    /// <summary>
    /// Mark a task as complete for a user
    /// </summary>
    Task<TaskResponseDto?> MarkTaskCompleteAsync(int id, int userId);

    /// <summary>
    /// Get all incomplete overdue tasks for a user
    /// </summary>
    Task<IEnumerable<TaskResponseDto>> GetOverdueTasksAsync(int userId);

    /// <summary>
    /// Delete a task for a user
    /// </summary>
    Task<bool> DeleteTaskAsync(int id, int userId);
}
