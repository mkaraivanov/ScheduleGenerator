using FluentAssertions;
using ScheduleGenerator.Application.Models;
using ScheduleGenerator.Application.Services;
using ScheduleGenerator.Domain.Entities;
using ScheduleGenerator.Domain.ValueObjects;

namespace ScheduleGenerator.Application.Tests.Services;

public class MatchGenerationServiceTests
{
    private readonly IMatchGenerationService _service;

    public MatchGenerationServiceTests()
    {
        _service = new MatchGenerationService();
    }

    [Fact]
    public void GenerateMatches_RoundRobin_4Teams_Generates6Matches()
    {
        // Arrange
        var format = TournamentFormat.RoundRobin();
        var matchDuration = new MatchDuration(90, 15);
        var tournament = new Tournament("Test Tournament", format, matchDuration);
        
        tournament.AddTeam(new Team("Team A"));
        tournament.AddTeam(new Team("Team B"));
        tournament.AddTeam(new Team("Team C"));
        tournament.AddTeam(new Team("Team D"));
        
        var formatConfig = new FormatConfiguration { Type = "RoundRobin" };

        // Act
        var matches = _service.GenerateMatches(tournament, formatConfig);

        // Assert
        // 4 teams round-robin: C(4,2) = 6 matches
        matches.Should().HaveCount(6);
        matches.Should().OnlyHaveUniqueItems(m => new { m.TeamAId, m.TeamBId });
    }

    [Fact]
    public void GenerateMatches_RoundRobin_EachTeamPlaysEachOtherOnce()
    {
        // Arrange
        var format = TournamentFormat.RoundRobin();
        var matchDuration = new MatchDuration(90, 15);
        var tournament = new Tournament("Test Tournament", format, matchDuration);
        
        var teamA = new Team("Team A");
        var teamB = new Team("Team B");
        var teamC = new Team("Team C");
        tournament.AddTeam(teamA);
        tournament.AddTeam(teamB);
        tournament.AddTeam(teamC);
        
        var formatConfig = new FormatConfiguration { Type = "RoundRobin" };

        // Act
        var matches = _service.GenerateMatches(tournament, formatConfig);

        // Assert
        // 3 teams: should have 3 matches
        matches.Should().HaveCount(3);
        
        // Each team should play exactly 2 matches
        var teams = new[] { teamA, teamB, teamC };
        foreach (var team in teams)
        {
            var teamMatches = matches.Count(m => m.TeamAId == team.Id || m.TeamBId == team.Id);
            teamMatches.Should().Be(2);
        }
    }

    [Fact]
    public void GenerateMatches_Groups_2Groups_GeneratesMatchesForEachGroup()
    {
        // Arrange
        var format = TournamentFormat.Groups(groupCount: 2, teamsAdvancingPerGroup: 1);
        var matchDuration = new MatchDuration(90, 15);
        var tournament = new Tournament("Test Tournament", format, matchDuration);
        
        tournament.AddTeam(new Team("Team A"));
        tournament.AddTeam(new Team("Team B"));
        tournament.AddTeam(new Team("Team C"));
        tournament.AddTeam(new Team("Team D"));
        
        var formatConfig = new FormatConfiguration
        {
            Type = "Groups",
            GroupStage = new GroupStageConfiguration
            {
                NumberOfGroups = 2
            }
        };

        // Act
        var matches = _service.GenerateMatches(tournament, formatConfig);

        // Assert
        // 2 groups of 2 teams each: 1 match per group = 2 matches total
        matches.Should().HaveCount(2);
        
        // Matches should be tagged with stage
        matches.Should().AllSatisfy(m => m.Stage.Should().Be(MatchStage.GroupStage));
    }

    [Fact]
    public void GenerateMatches_Knockout_8Teams_Generates7Matches()
    {
        // Arrange
        var format = TournamentFormat.Knockout();
        var matchDuration = new MatchDuration(90, 15);
        var tournament = new Tournament("Test Tournament", format, matchDuration);
        
        for (int i = 1; i <= 8; i++)
        {
            tournament.AddTeam(new Team($"Team {i}"));
        }
        
        var formatConfig = new FormatConfiguration
        {
            Type = "Knockout",
            Knockout = new KnockoutConfiguration
            {
                IncludeThirdPlaceMatch = false
            }
        };

        // Act
        var matches = _service.GenerateMatches(tournament, formatConfig);

        // Assert
        // 8-team knockout: 4 QF + 2 SF + 1 F = 7 matches
        matches.Should().HaveCount(7);
    }

    [Fact]
    public void GenerateMatches_InvalidFormat_ThrowsException()
    {
        // Arrange
        var format = TournamentFormat.RoundRobin();
        var matchDuration = new MatchDuration(90, 15);
        var tournament = new Tournament("Test Tournament", format, matchDuration);
        tournament.AddTeam(new Team("Team A"));
        tournament.AddTeam(new Team("Team B"));
        
        var formatConfig = new FormatConfiguration { Type = "InvalidFormat" };

        // Act & Assert
        var act = () => _service.GenerateMatches(tournament, formatConfig);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GenerateMatches_RoundRobin_2Teams_Generates1Match()
    {
        // Arrange
        var format = TournamentFormat.RoundRobin();
        var matchDuration = new MatchDuration(90, 15);
        var tournament = new Tournament("Test Tournament", format, matchDuration);
        
        tournament.AddTeam(new Team("Team A"));
        tournament.AddTeam(new Team("Team B"));
        
        var formatConfig = new FormatConfiguration { Type = "RoundRobin" };

        // Act
        var matches = _service.GenerateMatches(tournament, formatConfig);

        // Assert
        matches.Should().HaveCount(1);
    }
}
