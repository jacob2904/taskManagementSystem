using TaskManagement.API.DTOs;
using TaskManagement.Data.Entities;

namespace TaskManagement.API.Services.Interfaces;

/// <summary>
/// Service interface for user management operations
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Get user by ID
    /// </summary>
    Task<UserDto?> GetUserByIdAsync(int userId);

    /// <summary>
    /// Get user entity by ID
    /// </summary>
    Task<UserDetails?> GetUserEntityByIdAsync(int userId);

    /// <summary>
    /// Get user by email
    /// </summary>
    Task<UserDetails?> GetUserByEmailAsync(string email);

    /// <summary>
    /// Check if email exists
    /// </summary>
    Task<bool> EmailExistsAsync(string email);

    /// <summary>
    /// Update user's last login timestamp
    /// </summary>
    Task UpdateLastLoginAsync(int userId);
}
