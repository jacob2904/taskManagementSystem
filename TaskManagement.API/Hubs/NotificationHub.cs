using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Collections.Concurrent;

namespace TaskManagement.API.Hubs;

/// <summary>
/// SignalR hub for real-time task notifications with user-specific targeting
/// </summary>
[Authorize]
public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;
    private static readonly ConcurrentDictionary<string, HashSet<string>> UserConnections = new();

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        if (userId != null)
        {
            UserConnections.AddOrUpdate(
                userId,
                new HashSet<string> { Context.ConnectionId },
                (key, existingSet) =>
                {
                    existingSet.Add(Context.ConnectionId);
                    return existingSet;
                });

            _logger.LogInformation(
                "User {UserId} connected with ConnectionId: {ConnectionId}. Total connections for user: {Count}",
                userId,
                Context.ConnectionId,
                UserConnections[userId].Count);
        }
        else
        {
            _logger.LogWarning("Client connected without userId: {ConnectionId}", Context.ConnectionId);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        if (userId != null && UserConnections.TryGetValue(userId, out var connections))
        {
            connections.Remove(Context.ConnectionId);
            if (connections.Count == 0)
            {
                UserConnections.TryRemove(userId, out _);
            }

            _logger.LogInformation(
                "User {UserId} disconnected. ConnectionId: {ConnectionId}",
                userId,
                Context.ConnectionId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Get user ID from JWT claims
    /// </summary>
    private string? GetUserId()
    {
        return Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    /// <summary>
    /// Get all connection IDs for a specific user
    /// </summary>
    public static HashSet<string>? GetUserConnections(string userId)
    {
        return UserConnections.TryGetValue(userId, out var connections) ? connections : null;
    }
}
