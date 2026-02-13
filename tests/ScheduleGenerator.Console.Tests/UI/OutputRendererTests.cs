using FluentAssertions;
using ScheduleGenerator.Application.Models;
using ScheduleGenerator.Console.UI;
using Xunit;

namespace ScheduleGenerator.Console.Tests.UI;

public class OutputRendererTests
{
    [Fact]
    public void OutputRenderer_Should_BeInstantiable()
    {
        // Arrange & Act
        var renderer = new OutputRenderer();

        // Assert
        renderer.Should().NotBeNull();
    }

    [Fact]
    public void RenderSchedule_Should_HandleEmptySchedule()
    {
        // Arrange
        var renderer = new OutputRenderer();
        var schedule = new ScheduleOutput
        {
            TournamentName = "Test Tournament",
            Matches = new List<ScheduledMatchDto>(),
            Diagnostics = new ScheduleDiagnostics
            {
                IsValid = true,
                TotalMatches = 0,
                HardConstraintViolations = 0,
                SoftConstraintViolations = 0,
                Violations = new List<string>(),
                Warnings = new List<string>()
            }
        };

        // Act
        var act = () => renderer.RenderSchedule(schedule);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void RenderSchedule_Should_HandleScheduleWithMatches()
    {
        // Arrange
        var renderer = new OutputRenderer();
        var schedule = new ScheduleOutput
        {
            TournamentName = "Test Tournament",
            Matches = new List<ScheduledMatchDto>
            {
                new ScheduledMatchDto
                {
                    MatchId = 1,
                    HomeTeam = "Team A",
                    AwayTeam = "Team B",
                    Field = "Field 1",
                    StartTime = new DateTime(2026, 3, 15, 10, 0, 0),
                    EndTime = new DateTime(2026, 3, 15, 11, 0, 0),
                    Round = 1
                }
            },
            Diagnostics = new ScheduleDiagnostics
            {
                IsValid = true,
                TotalMatches = 1,
                HardConstraintViolations = 0,
                SoftConstraintViolations = 0,
                Violations = new List<string>(),
                Warnings = new List<string>()
            }
        };

        // Act
        var act = () => renderer.RenderSchedule(schedule);

        // Assert
        act.Should().NotThrow();
    }
}
