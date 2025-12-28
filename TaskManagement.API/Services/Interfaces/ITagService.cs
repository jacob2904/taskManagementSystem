using TaskManagement.API.DTOs;

namespace TaskManagement.API.Services.Interfaces;

/// <summary>
/// Service interface for tag operations
/// </summary>
public interface ITagService
{
    /// <summary>
    /// Get all tags for a user
    /// </summary>
    Task<IEnumerable<TagDto>> GetAllTagsAsync(int userId);

    /// <summary>
    /// Get tag by ID for a user
    /// </summary>
    Task<TagDto?> GetTagByIdAsync(int id, int userId);

    /// <summary>
    /// Create a new tag for a user
    /// </summary>
    Task<TagDto> CreateTagAsync(CreateTagDto createTagDto, int userId);

    /// <summary>
    /// Update an existing tag for a user
    /// </summary>
    Task<TagDto?> UpdateTagAsync(int id, CreateTagDto updateTagDto, int userId);

    /// <summary>
    /// Delete a tag for a user
    /// </summary>
    Task<bool> DeleteTagAsync(int id, int userId);

    /// <summary>
    /// Check if a tag with the given name exists for a user
    /// </summary>
    Task<bool> TagExistsAsync(string name, int userId, int? excludeId = null);
}
