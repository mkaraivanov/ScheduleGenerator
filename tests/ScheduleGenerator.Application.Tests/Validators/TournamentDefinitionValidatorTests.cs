using FluentAssertions;
using ScheduleGenerator.Application.Models;
using ScheduleGenerator.Application.Validators;

namespace ScheduleGenerator.Application.Tests.Validators;

public class TournamentDefinitionValidatorTests
{
    private readonly TournamentDefinitionValidator _validator;

    public TournamentDefinitionValidatorTests()
    {
        _validator = new TournamentDefinitionValidator();
    }

    [Fact]
    public void Validate_ValidRoundRobinDefinition_PassesValidation()
    {
        // Arrange
        var definition = new TournamentDefinition
        {
            Name = "Summer Cup",
            Teams = new List<TeamDefinition>
            {
                new() { Name = "Team A" },
                new() { Name = "Team B" },
                new() { Name = "Team C" }
            },
            Fields = new List<FieldDefinition>
            {
                new()
                {
                    Name = "Field 1",
                    AvailabilityWindows = new List<TimeWindow>
                    {
                        new() { Start = DateTime.Parse("2026-06-01T09:00:00"), End = DateTime.Parse("2026-06-01T18:00:00") }
                    }
                }
            },
            Format = new FormatConfiguration { Type = "RoundRobin" },
            Rules = new SchedulingRules
            {
                MatchDurationMinutes = 90,
                BufferBetweenMatchesMinutes = 15
            }
        };

        // Act
        var result = _validator.Validate(definition);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmptyTournamentName_FailsValidation()
    {
        // Arrange
        var definition = CreateValidDefinition();
        definition = definition with { Name = "" };

        // Act
        var result = _validator.Validate(definition);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Tournament name is required.");
    }

    [Fact]
    public void Validate_NoTeams_FailsValidation()
    {
        // Arrange
        var definition = CreateValidDefinition();
        definition = definition with { Teams = new List<TeamDefinition>() };

        // Act
        var result = _validator.Validate(definition);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "At least one team is required.");
    }

    [Fact]
    public void Validate_DuplicateTeamNames_FailsValidation()
    {
        // Arrange
        var definition = CreateValidDefinition();
        definition = definition with
        {
            Teams = new List<TeamDefinition>
            {
                new() { Name = "Team A" },
                new() { Name = "Team A" }
            }
        };

        // Act
        var result = _validator.Validate(definition);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Team names must be unique.");
    }

    [Fact]
    public void Validate_NoFields_FailsValidation()
    {
        // Arrange
        var definition = CreateValidDefinition();
        definition = definition with { Fields = new List<FieldDefinition>() };

        // Act
        var result = _validator.Validate(definition);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "At least one field is required.");
    }

    [Fact]
    public void Validate_KnockoutWithNonPowerOfTwo_FailsValidation()
    {
        // Arrange
        var definition = CreateValidDefinition();
        definition = definition with
        {
            Teams = new List<TeamDefinition>
            {
                new() { Name = "Team A" },
                new() { Name = "Team B" },
                new() { Name = "Team C" }
            },
            Format = new FormatConfiguration { Type = "Knockout" }
        };

        // Act
        var result = _validator.Validate(definition);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("power-of-2"));
    }

    [Fact]
    public void Validate_GroupStageWithInsufficientTeams_FailsValidation()
    {
        // Arrange
        var definition = CreateValidDefinition();
        definition = definition with
        {
            Teams = new List<TeamDefinition>
            {
                new() { Name = "Team A" },
                new() { Name = "Team B" }
            },
            Format = new FormatConfiguration
            {
                Type = "Groups",
                GroupStage = new GroupStageConfiguration
                {
                    NumberOfGroups = 2 // Requires at least 4 teams (2 per group)
                }
            }
        };

        // Act
        var result = _validator.Validate(definition);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("at least 2 teams per group"));
    }

    private TournamentDefinition CreateValidDefinition()
    {
        return new TournamentDefinition
        {
            Name = "Test Tournament",
            Teams = new List<TeamDefinition>
            {
                new() { Name = "Team A" },
                new() { Name = "Team B" }
            },
            Fields = new List<FieldDefinition>
            {
                new()
                {
                    Name = "Field 1",
                    AvailabilityWindows = new List<TimeWindow>
                    {
                        new() { Start = DateTime.Parse("2026-06-01T09:00:00"), End = DateTime.Parse("2026-06-01T18:00:00") }
                    }
                }
            },
            Format = new FormatConfiguration { Type = "RoundRobin" },
            Rules = new SchedulingRules
            {
                MatchDurationMinutes = 90,
                BufferBetweenMatchesMinutes = 15
            }
        };
    }
}
