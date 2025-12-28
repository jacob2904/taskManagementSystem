using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TaskManagement.API.DTOs;
using TaskManagement.API.Services.Interfaces;

namespace TaskManagement.API.Controllers;

/// <summary>
/// Controller for managing tasks
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;
    private readonly ILogger<TasksController> _logger;

    public TasksController(ITaskService taskService, ILogger<TasksController> logger)
    {
        _taskService = taskService;
        _logger = logger;
    }

    /// <summary>
    /// Get all tasks for authenticated user
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskResponseDto>>> GetTasks()
    {
        try
        {
            var userId = GetAuthenticatedUserId();
            var tasks = await _taskService.GetAllTasksAsync(userId);
            return Ok(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tasks");
            throw;
        }
    }

    /// <summary>
    /// Get task by ID for authenticated user
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<TaskResponseDto>> GetTask(int id)
    {
        try
        {
            var userId = GetAuthenticatedUserId();
            var task = await _taskService.GetTaskByIdAsync(id, userId);

            if (task == null)
            {
                return NotFound(new { message = $"Task with ID {id} not found" });
            }

            return Ok(task);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving task {TaskId}", id);
            throw;
        }
    }

    /// <summary>
    /// Create a new task for authenticated user
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<TaskResponseDto>> CreateTask(CreateTaskDto createTaskDto)
    {
        try
        {
            var userId = GetAuthenticatedUserId();
            var task = await _taskService.CreateTaskAsync(createTaskDto, userId);
            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("invalid"))
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating task");
            throw;
        }
    }

    /// <summary>
    /// Update an existing task for authenticated user
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<TaskResponseDto>> UpdateTask(int id, UpdateTaskDto updateTaskDto)
    {
        try
        {
            var userId = GetAuthenticatedUserId();
            var task = await _taskService.UpdateTaskAsync(id, updateTaskDto, userId);

            if (task == null)
            {
                return NotFound(new { message = $"Task with ID {id} not found" });
            }

            return Ok(task);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("invalid"))
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task {TaskId}", id);
            throw;
        }
    }

    /// <summary>
    /// Mark a task as complete for authenticated user
    /// </summary>
    [HttpPatch("{id}/complete")]
    public async Task<ActionResult<TaskResponseDto>> MarkTaskComplete(int id)
    {
        try
        {
            var userId = GetAuthenticatedUserId();
            var task = await _taskService.MarkTaskCompleteAsync(id, userId);

            if (task == null)
            {
                return NotFound(new { message = $"Task with ID {id} not found" });
            }

            return Ok(task);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking task {TaskId} as complete", id);
            throw;
        }
    }

    /// <summary>
    /// Get all incomplete overdue tasks for authenticated user
    /// </summary>
    [HttpGet("overdue")]
    public async Task<ActionResult<IEnumerable<TaskResponseDto>>> GetOverdueTasks()
    {
        try
        {
            var userId = GetAuthenticatedUserId();
            var overdueTasks = await _taskService.GetOverdueTasksAsync(userId);
            return Ok(overdueTasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving overdue tasks");
            throw;
        }
    }

    /// <summary>
    /// Delete a task for authenticated user
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        try
        {
            var userId = GetAuthenticatedUserId();
            var deleted = await _taskService.DeleteTaskAsync(id, userId);

            if (!deleted)
            {
                return NotFound(new { message = $"Task with ID {id} not found" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting task {TaskId}", id);
            throw;
        }
    }

    /// <summary>
    /// Get authenticated user ID from JWT claims
    /// </summary>
    private int GetAuthenticatedUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            throw new UnauthorizedAccessException("Invalid or missing user ID in token");
        }
        return userId;
    }
}
