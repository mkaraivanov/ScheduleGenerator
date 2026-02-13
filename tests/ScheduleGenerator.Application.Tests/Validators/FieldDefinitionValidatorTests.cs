using FluentAssertions;
using ScheduleGenerator.Application.Models;
using ScheduleGenerator.Application.Validators;

namespace ScheduleGenerator.Application.Tests.Validators;

public class FieldDefinitionValidatorTests
{
    private readonly FieldDefinitionValidator _validator;

    public FieldDefinitionValidatorTests()
    {
        _validator = new FieldDefinitionValidator();
    }

    [Fact]
    public void Validate_ValidFieldDefinition_PassesValidation()
    {
        // Arrange
        var field = new FieldDefinition
        {
            Name = "Field 1",
            AvailabilityWindows = new List<TimeWindow>
            {
                new() { Start = DateTime.Parse("2026-06-01T09:00:00"), End = DateTime.Parse("2026-06-01T18:00:00") }
            }
        };

        // Act
        var result = _validator.Validate(field);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmptyFieldName_FailsValidation()
    {
        // Arrange
        var field = new FieldDefinition
        {
            Name = "",
            AvailabilityWindows = new List<TimeWindow>
            {
                new() { Start = DateTime.Parse("2026-06-01T09:00:00"), End = DateTime.Parse("2026-06-01T18:00:00") }
            }
        };

        // Act
        var result = _validator.Validate(field);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Field name is required.");
    }

    [Fact]
    public void Validate_NoAvailabilityWindows_FailsValidation()
    {
        // Arrange
        var field = new FieldDefinition
        {
            Name = "Field 1",
            AvailabilityWindows = new List<TimeWindow>()
        };

        // Act
        var result = _validator.Validate(field);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("At least one availability window"));
    }

    [Fact]
    public void Validate_OverlappingWindows_FailsValidation()
    {
        // Arrange
        var field = new FieldDefinition
        {
            Name = "Field 1",
            AvailabilityWindows = new List<TimeWindow>
            {
                new() { Start = DateTime.Parse("2026-06-01T09:00:00"), End = DateTime.Parse("2026-06-01T12:00:00") },
                new() { Start = DateTime.Parse("2026-06-01T11:00:00"), End = DateTime.Parse("2026-06-01T14:00:00") }
            }
        };

        // Act
        var result = _validator.Validate(field);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("must not overlap"));
    }

    [Fact]
    public void Validate_NonOverlappingWindows_PassesValidation()
    {
        // Arrange
        var field = new FieldDefinition
        {
            Name = "Field 1",
            AvailabilityWindows = new List<TimeWindow>
            {
                new() { Start = DateTime.Parse("2026-06-01T09:00:00"), End = DateTime.Parse("2026-06-01T12:00:00") },
                new() { Start = DateTime.Parse("2026-06-01T14:00:00"), End = DateTime.Parse("2026-06-01T18:00:00") }
            }
        };

        // Act
        var result = _validator.Validate(field);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}

public class TimeWindowValidatorTests
{
    private readonly TimeWindowValidator _validator;

    public TimeWindowValidatorTests()
    {
        _validator = new TimeWindowValidator();
    }

    [Fact]
    public void Validate_ValidTimeWindow_PassesValidation()
    {
        // Arrange
        var window = new TimeWindow
        {
            Start = DateTime.Parse("2026-06-01T09:00:00"),
            End = DateTime.Parse("2026-06-01T18:00:00")
        };

        // Act
        var result = _validator.Validate(window);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EndBeforeStart_FailsValidation()
    {
        // Arrange
        var window = new TimeWindow
        {
            Start = DateTime.Parse("2026-06-01T18:00:00"),
            End = DateTime.Parse("2026-06-01T09:00:00")
        };

        // Act
        var result = _validator.Validate(window);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("End time must be after start time"));
    }
}
