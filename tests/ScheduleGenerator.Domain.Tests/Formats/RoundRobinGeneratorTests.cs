using FluentAssertions;
using ScheduleGenerator.Domain.Entities;
using ScheduleGenerator.Domain.Formats;

namespace ScheduleGenerator.Domain.Tests.Formats;

public class RoundRobinGeneratorTests
{
    [Fact]
    public void GenerateMatches_WithEvenNumberOfTeams_ShouldGenerateCorrectNumberOfMatches()
    {
        // Arrange
        var generator = new RoundRobinGenerator();
        var teams = new List<Team>
        {
            new Team("Team 1"),
            new Team("Team 2"),
            new Team("Team 3"),
            new Team("Team 4")
        };
        var format = TournamentFormat.RoundRobin(1);

        // Act
        var matches = generator.GenerateMatches(teams, format).ToList();

        // Assert
        // With 4 teams: (4 * 3) / 2 = 6 matches
        matches.Should().HaveCount(6);
    }

    [Fact]
    public void GenerateMatches_WithOddNumberOfTeams_ShouldGenerateCorrectNumberOfMatches()
    {
        // Arrange
        var generator = new RoundRobinGenerator();
        var teams = new List<Team>
        {
            new Team("Team 1"),
            new Team("Team 2"),
            new Team("Team 3"),
            new Team("Team 4"),
            new Team("Team 5")
        };
        var format = TournamentFormat.RoundRobin(1);

        // Act
        var matches = generator.GenerateMatches(teams, format).ToList();

        // Assert
        // With 5 teams: (5 * 4) / 2 = 10 matches (BYE team is handled internally)
        matches.Should().HaveCount(10);
    }

    [Fact]
    public void GenerateMatches_WithTwoLegs_ShouldDoubleTheMatches()
    {
        // Arrange
        var generator = new RoundRobinGenerator();
        var teams = new List<Team>
        {
            new Team("Team 1"),
            new Team("Team 2"),
            new Team("Team 3"),
            new Team("Team 4")
        };
        var format = TournamentFormat.RoundRobin(2);

        // Act
        var matches = generator.GenerateMatches(teams, format).ToList();

        // Assert
        // With 4 teams and 2 legs: 6 matches * 2 = 12 matches
        matches.Should().HaveCount(12);
    }

    [Fact]
    public void GenerateMatches_ShouldNotHaveTeamPlayingItself()
    {
        // Arrange
        var generator = new RoundRobinGenerator();
        var teams = new List<Team>
        {
            new Team("Team 1"),
            new Team("Team 2"),
            new Team("Team 3")
        };
        var format = TournamentFormat.RoundRobin(1);

        // Act
        var matches = generator.GenerateMatches(teams, format).ToList();

        // Assert
        matches.Should().NotContain(m => m.TeamAId == m.TeamBId);
    }

    [Fact]
    public void GenerateMatches_EachTeamShouldPlayEveryOtherTeamOnce()
    {
        // Arrange
        var generator = new RoundRobinGenerator();
        var teams = new List<Team>
        {
            new Team("Team 1"),
            new Team("Team 2"),
            new Team("Team 3")
        };
        var format = TournamentFormat.RoundRobin(1);

        // Act
        var matches = generator.GenerateMatches(teams, format).ToList();

        // Assert
        var teamIds = teams.Select(t => t.Id).ToList();
        
        foreach (var team in teamIds)
        {
            var opponentsForTeam = matches
                .Where(m => m.TeamAId == team || m.TeamBId == team)
                .SelectMany(m => new[] { m.TeamAId, m.TeamBId })
                .Where(id => id != team)
                .ToList();

            // Each team should play against every other team exactly once
            opponentsForTeam.Should().HaveCount(teamIds.Count - 1);
            opponentsForTeam.Distinct().Should().HaveCount(teamIds.Count - 1);
        }
    }

    [Fact]
    public void GenerateMatches_AllMatchesShouldHaveCorrectStage()
    {
        // Arrange
        var generator = new RoundRobinGenerator();
        var teams = new List<Team>
        {
            new Team("Team 1"),
            new Team("Team 2"),
            new Team("Team 3")
        };
        var format = TournamentFormat.RoundRobin(1);

        // Act
        var matches = generator.GenerateMatches(teams, format).ToList();

        // Assert
        matches.Should().AllSatisfy(m => m.Stage.Should().Be(MatchStage.RoundRobin));
    }

    [Fact]
    public void GenerateMatches_WithLessThanTwoTeams_ShouldThrowArgumentException()
    {
        // Arrange
        var generator = new RoundRobinGenerator();
        var teams = new List<Team> { new Team("Team 1") };
        var format = TournamentFormat.RoundRobin(1);

        // Act
        var act = () => generator.GenerateMatches(teams, format);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Round-robin requires at least 2 teams*");
    }

    [Fact]
    public void GenerateMatches_WithEmptyTeamList_ShouldThrowArgumentException()
    {
        // Arrange
        var generator = new RoundRobinGenerator();
        var teams = new List<Team>();
        var format = TournamentFormat.RoundRobin(1);

        // Act
        var act = () => generator.GenerateMatches(teams, format);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*At least one team is required*");
    }
}
