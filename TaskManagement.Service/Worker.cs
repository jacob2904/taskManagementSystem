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
    private Task? _consumerTask;

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
            _logger.LogInformation("RabbitMQService created successfully");

            _consumerTask = Task.Run(() => _rabbitMQService.StartConsumingAsync(cancellationToken), cancellationToken);

            _logger.LogInformation("RabbitMQ consumer started successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start RabbitMQ consumer");
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

            await Task.Delay(TimeSpan.FromMinutes(_config.CheckIntervalMinutes), stoppingToken);
        }
    }

    private async Task CheckOverdueTasksAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TaskManagementDbContext>();

        try
        {
            var now = DateTime.UtcNow;
            _logger.LogInformation("Checking for overdue tasks at: {CurrentTime}", now.ToString("yyyy-MM-dd HH:mm:ss"));
            
            var checkThreshold = now.AddMinutes(-_config.CheckIntervalMinutes);

            var overdueTasks = await context.Tasks
                .Where(t => !t.IsComplete && t.DueDate <= now)
                .Where(t => t.UpdatedAt == null || t.UpdatedAt < checkThreshold)
                .Select(t => new { t.Id, t.Title, t.DueDate, t.UserDetailsId, t.UpdatedAt })
                .ToListAsync();

            _logger.LogInformation("Query completed. Found {Count} overdue tasks matching criteria", overdueTasks.Count);
            
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
                _logger.LogInformation("Processing {Count} overdue tasks for notification", overdueTasks.Count);

                // Get the task IDs to update
                var taskIds = overdueTasks.Select(t => t.Id).ToList();

                foreach (var task in overdueTasks)
                {
                    if (_rabbitMQService != null)
                    {
                        await _rabbitMQService.PublishTaskReminderAsync(
                            task.Id,
                            task.UserDetailsId,
                            task.Title,
                            task.DueDate);
                    }
                }

                // Update the tasks in the database
                var tasksToUpdate = await context.Tasks
                    .Where(t => taskIds.Contains(t.Id))
                    .ToListAsync();

                foreach (var task in tasksToUpdate)
                {
                    task.UpdatedAt = DateTime.UtcNow;
                }

                await context.SaveChangesAsync();
            }
            else
            {
                _logger.LogInformation("No overdue tasks found at: {time}", DateTimeOffset.Now);
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

        if (_consumerTask != null)
        {
            await _consumerTask;
        }

        await base.StopAsync(cancellationToken);
    }
}

public class ServiceConfiguration
{
    public string RabbitMQHost { get; set; } = "localhost";
    public int CheckIntervalMinutes { get; set; } = 5;
}
