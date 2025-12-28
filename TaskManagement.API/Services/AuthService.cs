using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TaskManagement.API.DTOs;
using TaskManagement.API.Services.Interfaces;
using TaskManagement.Data;
using TaskManagement.Data.Entities;
using BCrypt.Net;

namespace TaskManagement.API.Services;

/// <summary>
/// Service for authentication operations
/// </summary>
public class AuthService : IAuthService
{
    private readonly TaskManagementDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IUserService _userService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        TaskManagementDbContext context,
        IConfiguration configuration,
        IUserService userService,
        ILogger<AuthService> logger)
    {
        _context = context;
        _configuration = configuration;
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto registerDto)
    {
        try
        {
            // Check if email already exists
            if (await _userService.EmailExistsAsync(registerDto.Email))
            {
                throw new InvalidOperationException("Email already registered");
            }

            // Hash password with BCrypt
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password, 12);

            // Create user entity
            var user = new UserDetails
            {
                Email = registerDto.Email,
                FullName = registerDto.FullName,
                Telephone = registerDto.Telephone,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow
            };

            _context.UserDetails.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User registered successfully: {Email}", registerDto.Email);

            // Generate JWT token
            var token = GenerateJwtToken(user);
            var expiresAt = DateTime.UtcNow.AddMinutes(GetTokenExpirationMinutes());

            return new AuthResponseDto
            {
                Token = token,
                User = MapToUserDto(user),
                ExpiresAt = expiresAt
            };
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration");
            throw new InvalidOperationException($"Error during registration: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Authenticate user and return JWT token
    /// </summary>
    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto loginDto)
    {
        try
        {
            // Get user by email
            var user = await _userService.GetUserByEmailAsync(loginDto.Email);

            if (user == null)
            {
                throw new InvalidOperationException("Invalid email or password");
            }

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                throw new InvalidOperationException("Invalid email or password");
            }

            // Update last login
            await _userService.UpdateLastLoginAsync(user.Id);

            _logger.LogInformation("User logged in successfully: {Email}", loginDto.Email);

            // Generate JWT token
            var token = GenerateJwtToken(user);
            var expiresAt = DateTime.UtcNow.AddMinutes(GetTokenExpirationMinutes());

            return new AuthResponseDto
            {
                Token = token,
                User = MapToUserDto(user),
                ExpiresAt = expiresAt
            };
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user login");
            throw new InvalidOperationException($"Error during login: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Change user password
    /// </summary>
    public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
    {
        try
        {
            var user = await _userService.GetUserEntityByIdAsync(userId);

            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            // Verify old password
            if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.OldPassword, user.PasswordHash))
            {
                throw new InvalidOperationException("Current password is incorrect");
            }

            // Hash new password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword, 12);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Password changed successfully for user {UserId}", userId);

            return true;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user {UserId}", userId);
            throw new InvalidOperationException($"Error changing password: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Validate JWT token and return user ID
    /// </summary>
    public int? ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT secret key not configured"));

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Generate JWT token for user
    /// </summary>
    private string GenerateJwtToken(UserDetails user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT secret key not configured"));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(GetTokenExpirationMinutes()),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    /// <summary>
    /// Get token expiration in minutes from configuration
    /// </summary>
    private int GetTokenExpirationMinutes()
    {
        return _configuration.GetValue<int>("Jwt:ExpirationMinutes", 60);
    }

    /// <summary>
    /// Map UserDetails entity to UserDto
    /// </summary>
    private static UserDto MapToUserDto(UserDetails user)
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
