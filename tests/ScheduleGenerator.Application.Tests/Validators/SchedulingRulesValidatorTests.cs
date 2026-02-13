using FluentAssertions;
using ScheduleGenerator.Application.Models;
using ScheduleGenerator.Application.Validators;

namespace ScheduleGenerator.Application.Tests.Validators;

public class SchedulingRulesValidatorTests
{
    private readonly SchedulingRulesValidator _validator;

    public SchedulingRulesValidatorTests()
    {
        _validator = new SchedulingRulesValidator();
    }

    [Fact]
    public void Validate_ValidRules_PassesValidation()
    {
        // Arrange
        var rules = new SchedulingRules
        {
            MatchDurationMinutes = 90,
            BufferBetweenMatchesMinutes = 15,
            MinimumRestTimeMinutes = 120
        };

        // Act
        var result = _validator.Validate(rules);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_NegativeMatchDuration_FailsValidation()
    {
        // Arrange
        var rules = new SchedulingRules
        {
            MatchDurationMinutes = -10,
            BufferBetweenMatchesMinutes = 15
        };

        // Act
        var result = _validator.Validate(rules);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Match duration must be greater than 0"));
    }

    [Fact]
    public void Validate_ExcessiveMatchDuration_FailsValidation()
    {
        // Arrange
        var rules = new SchedulingRules
        {
            MatchDurationMinutes = 700,
            BufferBetweenMatchesMinutes = 15
        };

        // Act
        var result = _validator.Validate(rules);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("must not exceed 600 minutes"));
    }

    [Fact]
    public void Validate_NegativeBufferTime_FailsValidation()
    {
        // Arrange
        var rules = new SchedulingRules
        {
            MatchDurationMinutes = 90,
            BufferBetweenMatchesMinutes = -5
        };

        // Act
        var result = _validator.Validate(rules);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Buffer time must be non-negative"));
    }

    [Fact]
    public void Validate_ExcessiveBufferTime_FailsValidation()
    {
        // Arrange
        var rules = new SchedulingRules
        {
            MatchDurationMinutes = 90,
            BufferBetweenMatchesMinutes = 150
        };

        // Act
        var result = _validator.Validate(rules);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("must not exceed 120 minutes"));
    }

    [Fact]
    public void Validate_NegativeRestTime_FailsValidation()
    {
        // Arrange
        var rules = new SchedulingRules
        {
            MatchDurationMinutes = 90,
            BufferBetweenMatchesMinutes = 15,
            MinimumRestTimeMinutes = -30
        };

        // Act
        var result = _validator.Validate(rules);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Minimum rest time must be non-negative"));
    }

    [Fact]
    public void Validate_InvalidMaxMatchesPerDay_FailsValidation()
    {
        // Arrange
        var rules = new SchedulingRules
        {
            MatchDurationMinutes = 90,
            BufferBetweenMatchesMinutes = 15,
            MaxMatchesPerTeamPerDay = 0
        };

        // Act
        var result = _validator.Validate(rules);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Maximum matches per team per day must be greater than 0"));
    }
}
