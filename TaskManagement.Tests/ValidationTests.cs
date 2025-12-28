using FluentValidation.TestHelper;
using TaskManagement.API.DTOs;
using TaskManagement.API.Validators;

namespace TaskManagement.Tests;

/// <summary>
/// Unit tests for FluentValidation validators
/// </summary>
public class ValidationTests
{
    private readonly CreateTaskDtoValidator _createTaskValidator;
    private readonly UpdateTaskDtoValidator _updateTaskValidator;
    private readonly UserDetailsDtoValidator _userDetailsValidator;
    private readonly CreateTagDtoValidator _createTagValidator;

    public ValidationTests()
    {
        _createTaskValidator = new CreateTaskDtoValidator();
        _updateTaskValidator = new UpdateTaskDtoValidator();
        _userDetailsValidator = new UserDetailsDtoValidator();
        _createTagValidator = new CreateTagDtoValidator();
    }

    [Fact]
    public void CreateTaskDto_ValidData_ShouldPassValidation()
    {
        // Arrange
        var dto = new CreateTaskDto
        {
            Title = "Valid Task Title",
            Description = "This is a valid task description with sufficient length",
            DueDate = DateTime.UtcNow.AddDays(7),
            Priority = 3,
            UserDetails = new UserDetailsDto
            {
                FullName = "John Doe",
                Telephone = "+1234567890",
                Email = "john.doe@example.com"
            },
            TagIds = new List<int> { 1, 2, 3 }
        };

        // Act
        var result = _createTaskValidator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateTaskDto_EmptyTitle_ShouldHaveValidationError()
    {
        // Arrange
        var dto = new CreateTaskDto
        {
            Title = "",
            Description = "Valid description here",
            DueDate = DateTime.UtcNow.AddDays(1),
            Priority = 3,
            UserDetails = new UserDetailsDto
            {
                FullName = "John Doe",
                Telephone = "+1234567890",
                Email = "john.doe@example.com"
            },
            TagIds = new List<int>()
        };

        // Act
        var result = _createTaskValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void CreateTaskDto_ShortDescription_ShouldHaveValidationError()
    {
        // Arrange
        var dto = new CreateTaskDto
        {
            Title = "Valid Title",
            Description = "Short",
            DueDate = DateTime.UtcNow.AddDays(1),
            Priority = 3,
            UserDetails = new UserDetailsDto
            {
                FullName = "John Doe",
                Telephone = "+1234567890",
                Email = "john.doe@example.com"
            },
            TagIds = new List<int>()
        };

        // Act
        var result = _createTaskValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void CreateTaskDto_InvalidPriority_ShouldHaveValidationError()
    {
        // Arrange
        var dto = new CreateTaskDto
        {
            Title = "Valid Title",
            Description = "This is a valid description",
            DueDate = DateTime.UtcNow.AddDays(1),
            Priority = 10,
            UserDetails = new UserDetailsDto
            {
                FullName = "John Doe",
                Telephone = "+1234567890",
                Email = "john.doe@example.com"
            },
            TagIds = new List<int>()
        };

        // Act
        var result = _createTaskValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Priority);
    }

    [Fact]
    public void UserDetailsDto_InvalidEmail_ShouldHaveValidationError()
    {
        // Arrange
        var dto = new UserDetailsDto
        {
            FullName = "John Doe",
            Telephone = "+1234567890",
            Email = "invalid-email"
        };

        // Act
        var result = _userDetailsValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void UserDetailsDto_ShortFullName_ShouldHaveValidationError()
    {
        // Arrange
        var dto = new UserDetailsDto
        {
            FullName = "J",
            Telephone = "+1234567890",
            Email = "john@example.com"
        };

        // Act
        var result = _userDetailsValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FullName);
    }

    [Fact]
    public void CreateTagDto_ValidName_ShouldPassValidation()
    {
        // Arrange
        var dto = new CreateTagDto
        {
            Name = "Urgent"
        };

        // Act
        var result = _createTagValidator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateTagDto_EmptyName_ShouldHaveValidationError()
    {
        // Arrange
        var dto = new CreateTagDto
        {
            Name = ""
        };

        // Act
        var result = _createTagValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void CreateTagDto_TooLongName_ShouldHaveValidationError()
    {
        // Arrange
        var dto = new CreateTagDto
        {
            Name = new string('A', 51)
        };

        // Act
        var result = _createTagValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void CreateTaskDto_ValidPriorities_ShouldPassValidation(int priority)
    {
        // Arrange
        var dto = new CreateTaskDto
        {
            Title = "Valid Task",
            Description = "This is a valid task description",
            DueDate = DateTime.UtcNow.AddDays(1),
            Priority = priority,
            UserDetails = new UserDetailsDto
            {
                FullName = "John Doe",
                Telephone = "+1234567890",
                Email = "john@example.com"
            },
            TagIds = new List<int>()
        };

        // Act
        var result = _createTaskValidator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Priority);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    [InlineData(100)]
    public void CreateTaskDto_InvalidPriorities_ShouldHaveValidationError(int priority)
    {
        // Arrange
        var dto = new CreateTaskDto
        {
            Title = "Valid Task",
            Description = "This is a valid task description",
            DueDate = DateTime.UtcNow.AddDays(1),
            Priority = priority,
            UserDetails = new UserDetailsDto
            {
                FullName = "John Doe",
                Telephone = "+1234567890",
                Email = "john@example.com"
            },
            TagIds = new List<int>()
        };

        // Act
        var result = _createTaskValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Priority);
    }
}
