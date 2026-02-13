using System.Text.Json;
using FluentAssertions;
using ScheduleGenerator.Application.Models;

namespace ScheduleGenerator.Application.Tests.Models;

public class TournamentDefinitionTests
{
    [Fact]
    public void TournamentDefinition_ShouldSerializeAndDeserialize()
    {
        // Arrange
        var definition = new TournamentDefinition
        {
            Name = "Summer Cup",
            Teams = new List<TeamDefinition>
            {
                new() { Name = "Team A", Seed = 1 },
                new() { Name = "Team B", Seed = 2 }
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
            Format = new FormatConfiguration
            {
                Type = "RoundRobin"
            },
            Rules = new SchedulingRules
            {
                MatchDurationMinutes = 90,
                BufferBetweenMatchesMinutes = 15
            }
        };

        // Act
        var json = JsonSerializer.Serialize(definition);
        var deserialized = JsonSerializer.Deserialize<TournamentDefinition>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Name.Should().Be("Summer Cup");
        deserialized.Teams.Should().HaveCount(2);
        deserialized.Fields.Should().HaveCount(1);
    }

    [Fact]
    public void TeamDefinition_WithSeed_ShouldRetainValue()
    {
        // Arrange & Act
        var team = new TeamDefinition { Name = "Team A", Seed = 1 };

        // Assert
        team.Name.Should().Be("Team A");
        team.Seed.Should().Be(1);
    }

    [Fact]
    public void TeamDefinition_WithoutSeed_ShouldBeNull()
    {
        // Arrange & Act
        var team = new TeamDefinition { Name = "Team B", Seed = null };

        // Assert
        team.Seed.Should().BeNull();
    }
}
