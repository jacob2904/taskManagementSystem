using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TaskManagement.Data;
using TaskManagement.Service.Services;

namespace TaskManagement.Service;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly ServiceConfiguration _config;
    private RabbitMQService? _rabbitMQService;

    public Worker(
        ILogger<Worker> logger,
        IServiceProvider serviceProvider,
        IOptions<ServiceConfiguration> config)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _config = config.Value;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Task Management Service starting at: {time}", DateTimeOffset.Now);

        try
        {
            _logger.LogInformation("Creating RabbitMQService with host: {Host}", _config.RabbitMQHost);
            _rabbitMQService = new RabbitMQService(
                _serviceProvider.GetRequiredService<ILogger<RabbitMQService>>(),
                _config.RabbitMQHost);
            _logger.LogInformation("RabbitMQService created successfully (publisher only - API handles consumption)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create RabbitMQ service");
        }

        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Task Management Service is running");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckOverdueTasksAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking overdue tasks");
            }

            await Task.Delay(TimeSpan.FromSeconds(_config.CheckIntervalSeconds), stoppingToken);
        }
    }

    private async Task CheckOverdueTasksAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TaskManagementDbContext>();

        try
        {
            var now = DateTime.UtcNow;
            _logger.LogInformation("Checking for due/overdue tasks at: {CurrentTime}", now.ToString("yyyy-MM-dd HH:mm:ss"));

            // Get tasks that are due or overdue and haven't been notified yet
            // A task is eligible for notification if:
            // 1. Not complete AND due date has passed (DueDate <= now)
            // 2. UpdatedAt is null (never notified) OR UpdatedAt is before DueDate (was updated before becoming due)
            // This ensures each task only gets ONE notification when it becomes due/overdue
            var overdueTasks = await context.Tasks
                .Where(t => !t.IsComplete && t.DueDate <= now)
                .Where(t => t.UpdatedAt == null || t.UpdatedAt < t.DueDate)
                .OrderBy(t => t.DueDate)
                .Select(t => new { t.Id, t.Title, t.DueDate, t.UserDetailsId, t.UpdatedAt })
                .ToListAsync();

            _logger.LogInformation("Query completed. Found {Count} due/overdue tasks that need notification", overdueTasks.Count);
            
            foreach (var task in overdueTasks)
            {
                _logger.LogInformation(
                    "  - Task ID {TaskId}: '{Title}' | Due: {DueDate} | UpdatedAt: {UpdatedAt}",
                    task.Id,
                    task.Title,
                    task.DueDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    task.UpdatedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "null");
            }

            if (overdueTasks.Any())
            {
                _logger.LogInformation("Publishing {Count} task notification(s) to RabbitMQ", overdueTasks.Count);

                // Publish each task to RabbitMQ individually
                // Each task will be sent to SignalR and marked as notified by the API service
                foreach (var task in overdueTasks)
                {
                    if (_rabbitMQService != null)
                    {
                        _logger.LogInformation(
                            "Publishing notification for Task ID {TaskId}: '{Title}' (Due: {DueDate})",
                            task.Id,
                            task.Title,
                            task.DueDate.ToString("yyyy-MM-dd HH:mm:ss"));
                        
                        await _rabbitMQService.PublishTaskReminderAsync(
                            task.Id,
                            task.UserDetailsId,
                            task.Title,
                            task.DueDate);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing overdue tasks");
            throw;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Task Management Service is stopping");

        _rabbitMQService?.Dispose();

        await base.StopAsync(cancellationToken);
    }
}

public class ServiceConfiguration
{
    public string RabbitMQHost { get; set; } = "localhost";
    public int CheckIntervalSeconds { get; set; } = 300;
}
