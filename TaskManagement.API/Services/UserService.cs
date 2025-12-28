using Microsoft.EntityFrameworkCore;
using TaskManagement.API.DTOs;
using TaskManagement.API.Services.Interfaces;
using TaskManagement.Data;
using TaskManagement.Data.Entities;

namespace TaskManagement.API.Services;

/// <summary>
/// Service for user management operations
/// </summary>
public class UserService : IUserService
{
    private readonly TaskManagementDbContext _context;
    private readonly ILogger<UserService> _logger;

    public UserService(TaskManagementDbContext context, ILogger<UserService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    public async Task<UserDto?> GetUserByIdAsync(int userId)
    {
        try
        {
            var user = await _context.UserDetails
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return null;
            }

            return MapToDto(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user {UserId}", userId);
            throw new InvalidOperationException($"Error retrieving user: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Get user entity by ID
    /// </summary>
    public async Task<UserDetails?> GetUserEntityByIdAsync(int userId)
    {
        try
        {
            return await _context.UserDetails
                .FirstOrDefaultAsync(u => u.Id == userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user entity {UserId}", userId);
            throw new InvalidOperationException($"Error retrieving user entity: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Get user by email
    /// </summary>
    public async Task<UserDetails?> GetUserByEmailAsync(string email)
    {
        try
        {
            return await _context.UserDetails
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user by email");
            throw new InvalidOperationException($"Error retrieving user by email: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Check if email exists
    /// </summary>
    public async Task<bool> EmailExistsAsync(string email)
    {
        try
        {
            return await _context.UserDetails
                .AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking email existence");
            throw new InvalidOperationException($"Error checking email existence: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Update user's last login timestamp
    /// </summary>
    public async Task UpdateLastLoginAsync(int userId)
    {
        try
        {
            var user = await _context.UserDetails.FindAsync(userId);
            if (user != null)
            {
                user.LastLoginAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating last login for user {UserId}", userId);
            throw new InvalidOperationException($"Error updating last login: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Map UserDetails entity to UserDto
    /// </summary>
    private static UserDto MapToDto(UserDetails user)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Telephone = user.Telephone,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        };
    }
}
