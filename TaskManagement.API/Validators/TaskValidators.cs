using FluentValidation;
using TaskManagement.API.DTOs;
using static TaskManagement.API.Services.TimezoneService;

namespace TaskManagement.API.Validators;

/// <summary>
/// Validator for UserDetailsDto
/// </summary>
public class UserDetailsDtoValidator : AbstractValidator<UserDetailsDto>
{
    public UserDetailsDtoValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required")
            .Length(2, 200).WithMessage("Full name must be between 2 and 200 characters");

        RuleFor(x => x.Telephone)
            .NotEmpty().WithMessage("Telephone is required")
            .Matches(@"^[\d\s\-\+\(\)]+$").WithMessage("Invalid telephone number format")
            .MaximumLength(20).WithMessage("Telephone cannot exceed 20 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email address format")
            .MaximumLength(100).WithMessage("Email cannot exceed 100 characters");
    }
}

/// <summary>
/// Validator for CreateTaskDto
/// </summary>
public class CreateTaskDtoValidator : AbstractValidator<CreateTaskDto>
{
    public CreateTaskDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .Length(3, 200).WithMessage("Title must be between 3 and 200 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .Length(10, 2000).WithMessage("Description must be between 10 and 2000 characters");

        RuleFor(x => x.DueDate)
            .NotEmpty().WithMessage("Due date is required")
            .Must(date => date.ToUniversalTime() > DateTime.UtcNow.AddHours(-24))
            .WithMessage("Due date cannot be more than 24 hours in the past");

        RuleFor(x => x.Priority)
            .InclusiveBetween(1, 5).WithMessage("Priority must be between 1 and 5");

        RuleFor(x => x.TagIds)
            .NotNull().WithMessage("Tag IDs must be provided")
            .Must(tags => tags == null || tags.All(id => id > 0))
            .WithMessage("All tag IDs must be positive integers");
    }
}

/// <summary>
/// Validator for UpdateTaskDto
/// </summary>
public class UpdateTaskDtoValidator : AbstractValidator<UpdateTaskDto>
{
    public UpdateTaskDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .Length(3, 200).WithMessage("Title must be between 3 and 200 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .Length(10, 2000).WithMessage("Description must be between 10 and 2000 characters");

        RuleFor(x => x.DueDate)
            .NotEmpty().WithMessage("Due date is required");

        RuleFor(x => x.Priority)
            .InclusiveBetween(1, 5).WithMessage("Priority must be between 1 and 5");

        RuleFor(x => x.TagIds)
            .NotNull().WithMessage("Tag IDs must be provided")
            .Must(tags => tags == null || tags.All(id => id > 0))
            .WithMessage("All tag IDs must be positive integers");
    }
}

/// <summary>
/// Validator for CreateTagDto
/// </summary>
public class CreateTagDtoValidator : AbstractValidator<CreateTagDto>
{
    public CreateTagDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tag name is required")
            .Length(2, 50).WithMessage("Tag name must be between 2 and 50 characters");
    }
}
