using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using TaskManagement.API.Hubs;
using TaskManagement.Data;

namespace TaskManagement.API.Services;

/// <summary>
/// Background service that consumes RabbitMQ messages and broadcasts via SignalR
/// </summary>
public class RabbitMQConsumerService : BackgroundService
{
    private readonly ILogger<RabbitMQConsumerService> _logger;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;
    private IConnection? _connection;
    private IChannel? _channel;
    private const string QueueName = "TaskReminders";

    public RabbitMQConsumerService(
        ILogger<RabbitMQConsumerService> logger,
        IHubContext<NotificationHub> hubContext,
        IConfiguration configuration,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _hubContext = hubContext;
        _configuration = configuration;
        _serviceProvider = serviceProvider;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMQ:HostName"] ?? "localhost",
                Port = _configuration.GetValue<int>("RabbitMQ:Port", 5672),
                UserName = _configuration["RabbitMQ:UserName"] ?? "guest",
                Password = _configuration["RabbitMQ:Password"] ?? "guest"
            };

            _connection = await factory.CreateConnectionAsync(cancellationToken);
            _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

            await _channel.QueueDeclareAsync(
                queue: QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            _logger.LogInformation("RabbitMQ consumer connected successfully");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to connect to RabbitMQ. Service will not process messages until RabbitMQ is available. Error: {Message}", ex.Message);
        }

        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_channel == null)
        {
            _logger.LogWarning("RabbitMQ channel is not initialized. Skipping message consumption. Ensure RabbitMQ is running and accessible.");
            return;
        }

        try
        {
            await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);
                    var message = JsonSerializer.Deserialize<TaskReminderMessage>(json);

                    if (message != null)
                    {
                        _logger.LogInformation(
                            "Received notification: Task '{TaskTitle}' (ID: {TaskId}) for User {UserId} - Due Date: {DueDate}",
                            message.TaskTitle,
                            message.TaskId,
                            message.UserId,
                            message.DueDate.ToString("yyyy-MM-dd HH:mm:ss"));

                        bool deliverySuccessful = false;

                        // Get user's connection IDs
                        var userConnections = NotificationHub.GetUserConnections(message.UserId.ToString());
                        
                        if (userConnections != null && userConnections.Count > 0)
                        {
                            // Send to specific user's connections only
                            await _hubContext.Clients.Clients(userConnections).SendAsync(
                                "ReceiveTaskNotification",
                                new
                                {
                                    taskId = message.TaskId,
                                    taskTitle = message.TaskTitle,
                                    dueDate = message.DueDate,
                                    timestamp = message.Timestamp,
                                    message = $"Task '{message.TaskTitle}' is overdue!"
                                },
                                stoppingToken);

                            _logger.LogInformation(
                                "Notification sent via SignalR to user {UserId} ({ConnectionCount} connection(s)).",
                                message.UserId,
                                userConnections.Count);
                            
                            deliverySuccessful = true;
                        }
                        else
                        {
                            _logger.LogInformation(
                                "User {UserId} is not connected. Notification not delivered.",
                                message.UserId);
                            deliverySuccessful = true; // Still mark as delivered to prevent retries
                        }

                        // Only mark task as notified and remove from queue after successful delivery
                        if (deliverySuccessful)
                        {
                            using (var scope = _serviceProvider.CreateScope())
                            {
                                var context = scope.ServiceProvider.GetRequiredService<TaskManagementDbContext>();
                                var task = await context.Tasks.FindAsync(message.TaskId);
                                
                                if (task != null)
                                {
                                    task.UpdatedAt = DateTime.UtcNow;
                                    await context.SaveChangesAsync();
                                    
                                    _logger.LogInformation(
                                        "Task ID {TaskId} marked as notified (UpdatedAt set).",
                                        message.TaskId);
                                }
                            }

                            // Acknowledge and permanently remove message from queue
                            await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
                            _logger.LogInformation(
                                "Message for Task ID {TaskId} successfully removed from RabbitMQ queue",
                                message.TaskId);
                        }
                    }
                    else
                    {
                        // Invalid message - remove it from queue
                        await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
                        _logger.LogWarning("Received invalid/null message. Removed from queue.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing RabbitMQ message");
                    await _channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                }
            };

            await _channel.BasicConsumeAsync(
                queue: QueueName,
                autoAck: false,
                consumer: consumer);

            _logger.LogInformation("Started consuming messages from queue: {QueueName}", QueueName);

            // Keep the service running
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in RabbitMQ consumer");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping RabbitMQ consumer service");
        
        if (_channel != null)
        {
            await _channel.CloseAsync();
            _channel.Dispose();
        }
        if (_connection != null)
        {
            await _connection.CloseAsync();
            _connection.Dispose();
        }

        await base.StopAsync(cancellationToken);
    }
}

/// <summary>
/// Message model for task reminders
/// </summary>
public class TaskReminderMessage
{
    public int TaskId { get; set; }
    public int UserId { get; set; }
    public string TaskTitle { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public DateTime Timestamp { get; set; }
}
