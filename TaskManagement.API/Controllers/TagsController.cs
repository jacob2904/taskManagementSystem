using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TaskManagement.API.DTOs;
using TaskManagement.API.Services.Interfaces;

namespace TaskManagement.API.Controllers;

/// <summary>
/// Controller for managing tags
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TagsController : ControllerBase
{
    private readonly ITagService _tagService;
    private readonly ILogger<TagsController> _logger;

    public TagsController(ITagService tagService, ILogger<TagsController> logger)
    {
        _tagService = tagService;
        _logger = logger;
    }

    /// <summary>
    /// Get all tags for authenticated user
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TagDto>>> GetTags()
    {
        try
        {
            var userId = GetAuthenticatedUserId();
            var tags = await _tagService.GetAllTagsAsync(userId);
            return Ok(tags);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tags");
            throw;
        }
    }

    /// <summary>
    /// Get tag by ID for authenticated user
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<TagDto>> GetTag(int id)
    {
        try
        {
            var userId = GetAuthenticatedUserId();
            var tag = await _tagService.GetTagByIdAsync(id, userId);

            if (tag == null)
            {
                return NotFound(new { message = $"Tag with ID {id} not found" });
            }

            return Ok(tag);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tag {TagId}", id);
            throw;
        }
    }

    /// <summary>
    /// Create a new tag for authenticated user
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<TagDto>> CreateTag(CreateTagDto createTagDto)
    {
        try
        {
            var userId = GetAuthenticatedUserId();
            var tag = await _tagService.CreateTagAsync(createTagDto, userId);
            return CreatedAtAction(nameof(GetTag), new { id = tag.Id }, tag);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tag");
            throw;
        }
    }

    /// <summary>
    /// Update an existing tag for authenticated user
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<TagDto>> UpdateTag(int id, CreateTagDto updateTagDto)
    {
        try
        {
            var userId = GetAuthenticatedUserId();
            var tag = await _tagService.UpdateTagAsync(id, updateTagDto, userId);

            if (tag == null)
            {
                return NotFound(new { message = $"Tag with ID {id} not found" });
            }

            return Ok(tag);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tag {TagId}", id);
            throw;
        }
    }

    /// <summary>
    /// Delete a tag for authenticated user
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTag(int id)
    {
        try
        {
            var userId = GetAuthenticatedUserId();
            var deleted = await _tagService.DeleteTagAsync(id, userId);

            if (!deleted)
            {
                return NotFound(new { message = $"Tag with ID {id} not found" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting tag {TagId}", id);
            throw;
        }
    }

    /// <summary>
    /// Get authenticated user ID from JWT claims
    /// </summary>
    private int GetAuthenticatedUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            throw new UnauthorizedAccessException("Invalid or missing user ID in token");
        }
        return userId;
    }
}
