using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace TaskManagement.Service.Services;

/// <summary>
/// Service for RabbitMQ message publishing and consuming
/// </summary>
public class RabbitMQService : IDisposable
{
    private readonly ILogger<RabbitMQService> _logger;
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private const string QueueName = "TaskReminders";

    public RabbitMQService(ILogger<RabbitMQService> logger, string hostName = "localhost")
    {
        _logger = logger;

        try
        {
            var factory = new ConnectionFactory
            {
                HostName = hostName,
                Port = 5672,
                UserName = "guest",
                Password = "guest",
                DispatchConsumersAsync = true
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(
                queue: QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            _logger.LogInformation("RabbitMQ connection established successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to RabbitMQ");
            throw;
        }
    }

    /// <summary>
    /// Publish a task reminder message to the queue
    /// </summary>
    public async Task PublishTaskReminderAsync(int taskId, int userId, string taskTitle, DateTime dueDate)
    {
        await Task.Run(() =>
        {
            try
            {
                var message = new TaskReminderMessage
                {
                    TaskId = taskId,
                    UserId = userId,
                    TaskTitle = taskTitle,
                    DueDate = dueDate,
                    Timestamp = DateTime.UtcNow
                };

                var json = JsonSerializer.Serialize(message);
                var body = Encoding.UTF8.GetBytes(json);

                var properties = _channel.CreateBasicProperties();
                properties.Persistent = true;

                _channel.BasicPublish(
                    exchange: string.Empty,
                    routingKey: QueueName,
                    basicProperties: properties,
                    body: body);

                _logger.LogInformation("Published reminder for Task ID: {TaskId}", taskId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing task reminder for Task ID: {TaskId}", taskId);
                throw;
            }
        });
    }

    /// <summary>
    /// Start consuming messages from the queue
    /// </summary>
    public async Task StartConsumingAsync(CancellationToken cancellationToken)
    {
        await Task.Run(() =>
        {
            try
            {
                _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                var consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.Received += async (model, ea) =>
                {
                    try
                    {
                        var body = ea.Body.ToArray();
                        var json = Encoding.UTF8.GetString(body);
                        var message = JsonSerializer.Deserialize<TaskReminderMessage>(json);

                        if (message != null)
                        {
                            _logger.LogInformation(
                                "NOTIFICATION SENT: Task '{TaskTitle}' (ID: {TaskId}) is overdue - Due Date: {DueDate}",
                                message.TaskTitle,
                                message.TaskId,
                                message.DueDate.ToString("yyyy-MM-dd HH:mm:ss"));
                        }

                        // Acknowledge and remove from queue
                        _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                        _logger.LogInformation("Notification removed from queue for Task ID: {TaskId}", message?.TaskId);
                        await Task.CompletedTask;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing message");
                        _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
                    }
                };

                _channel.BasicConsume(
                    queue: QueueName,
                    autoAck: false,
                    consumer: consumer);

                _logger.LogInformation("Started consuming messages from queue: {QueueName}", QueueName);

                while (!cancellationToken.IsCancellationRequested)
                {
                    Task.Delay(1000, cancellationToken).Wait(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in message consumer");
                throw;
            }
        }, cancellationToken);
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
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
