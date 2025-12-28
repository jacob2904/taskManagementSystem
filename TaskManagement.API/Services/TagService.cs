using Microsoft.EntityFrameworkCore;
using TaskManagement.API.DTOs;
using TaskManagement.API.Services.Interfaces;
using TaskManagement.Data;
using TaskManagement.Data.Entities;

namespace TaskManagement.API.Services;

/// <summary>
/// Service for tag operations
/// </summary>
public class TagService : ITagService
{
    private readonly TaskManagementDbContext _context;
    private readonly ILogger<TagService> _logger;

    public TagService(TaskManagementDbContext context, ILogger<TagService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all tags for a user
    /// </summary>
    public async Task<IEnumerable<TagDto>> GetAllTagsAsync(int userId)
    {
        try
        {
            var tags = await _context.Tags
                .Where(t => t.UserDetailsId == userId)
                .Select(t => new TagDto
                {
                    Id = t.Id,
                    Name = t.Name
                })
                .ToListAsync();

            return tags;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tags for user {UserId}", userId);
            throw new InvalidOperationException("An error occurred while retrieving tags", ex);
        }
    }

    /// <summary>
    /// Get tag by ID for a user
    /// </summary>
    public async Task<TagDto?> GetTagByIdAsync(int id, int userId)
    {
        try
        {
            var tag = await _context.Tags
                .Where(t => t.Id == id && t.UserDetailsId == userId)
                .Select(t => new TagDto
                {
                    Id = t.Id,
                    Name = t.Name
                })
                .FirstOrDefaultAsync();

            return tag;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tag {TagId} for user {UserId}", id, userId);
            throw new InvalidOperationException($"An error occurred while retrieving tag {id}", ex);
        }
    }

    /// <summary>
    /// Create a new tag for a user
    /// </summary>
    public async Task<TagDto> CreateTagAsync(CreateTagDto createTagDto, int userId)
    {
        try
        {
            if (await TagExistsAsync(createTagDto.Name, userId))
            {
                throw new InvalidOperationException("A tag with this name already exists");
            }

            var tag = new Tag
            {
                Name = createTagDto.Name,
                UserDetailsId = userId
            };

            _context.Tags.Add(tag);
            await _context.SaveChangesAsync();

            return new TagDto
            {
                Id = tag.Id,
                Name = tag.Name
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tag for user {UserId}", userId);
            throw new InvalidOperationException("An error occurred while creating the tag", ex);
        }
    }

    /// <summary>
    /// Update an existing tag for a user
    /// </summary>
    public async Task<TagDto?> UpdateTagAsync(int id, CreateTagDto updateTagDto, int userId)
    {
        try
        {
            var tag = await _context.Tags
                .FirstOrDefaultAsync(t => t.Id == id && t.UserDetailsId == userId);

            if (tag == null)
            {
                return null;
            }

            if (await TagExistsAsync(updateTagDto.Name, userId, id))
            {
                throw new InvalidOperationException("A tag with this name already exists");
            }

            tag.Name = updateTagDto.Name;

            await _context.SaveChangesAsync();

            return new TagDto
            {
                Id = tag.Id,
                Name = tag.Name
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tag {TagId} for user {UserId}", id, userId);
            throw new InvalidOperationException($"An error occurred while updating tag {id}", ex);
        }
    }

    /// <summary>
    /// Delete a tag for a user
    /// </summary>
    public async Task<bool> DeleteTagAsync(int id, int userId)
    {
        try
        {
            var tag = await _context.Tags
                .FirstOrDefaultAsync(t => t.Id == id && t.UserDetailsId == userId);

            if (tag == null)
            {
                return false;
            }

            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting tag {TagId} for user {UserId}", id, userId);
            throw new InvalidOperationException($"An error occurred while deleting tag {id}", ex);
        }
    }

    /// <summary>
    /// Check if a tag with the given name exists for a user
    /// </summary>
    public async Task<bool> TagExistsAsync(string name, int userId, int? excludeId = null)
    {
        try
        {
            var query = _context.Tags.Where(t => t.Name == name && t.UserDetailsId == userId);

            if (excludeId.HasValue)
            {
                query = query.Where(t => t.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if tag exists for user {UserId}", userId);
            throw new InvalidOperationException("An error occurred while checking if tag exists", ex);
        }
    }
}
