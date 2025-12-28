using TaskManagement.API.DTOs;

namespace TaskManagement.API.Services.Interfaces;

/// <summary>
/// Service interface for authentication operations
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Register a new user
    /// </summary>
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto registerDto);

    /// <summary>
    /// Authenticate a user and return JWT token
    /// </summary>
    Task<AuthResponseDto> LoginAsync(LoginRequestDto loginDto);

    /// <summary>
    /// Change user password
    /// </summary>
    Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto);

    /// <summary>
    /// Validate JWT token and return user ID
    /// </summary>
    int? ValidateToken(string token);
}
