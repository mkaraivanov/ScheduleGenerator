using FluentAssertions;
using ScheduleGenerator.Domain.Entities;
using ScheduleGenerator.Domain.Formats;

namespace ScheduleGenerator.Domain.Tests.Formats;

public class KnockoutGeneratorTests
{
    [Fact]
    public void GenerateMatches_WithPowerOfTwoTeams_ShouldGenerateCorrectNumberOfMatches()
    {
        // Arrange
        var generator = new KnockoutGenerator();
        var teams = new List<Team>
        {
            new Team("Team 1"),
            new Team("Team 2"),
            new Team("Team 3"),
            new Team("Team 4")
        };
        var format = TournamentFormat.Knockout();

        // Act
        var matches = generator.GenerateMatches(teams, format).ToList();

        // Assert
        // With 4 teams: 2 semi-finals + 1 final = 3 matches
        matches.Should().HaveCount(3);
    }

    [Fact]
    public void GenerateMatches_WithEightTeams_ShouldGenerateCorrectBracket()
    {
        // Arrange
        var generator = new KnockoutGenerator();
        var teams = new List<Team>
        {
            new Team("Team 1"),
            new Team("Team 2"),
            new Team("Team 3"),
            new Team("Team 4"),
            new Team("Team 5"),
            new Team("Team 6"),
            new Team("Team 7"),
            new Team("Team 8")
        };
        var format = TournamentFormat.Knockout();

        // Act
        var matches = generator.GenerateMatches(teams, format).ToList();

        // Assert
        // With 8 teams: 4 quarter-finals + 2 semi-finals + 1 final = 7 matches
        matches.Should().HaveCount(7);
    }

    [Fact]
    public void GenerateMatches_WithNonPowerOfTwoTeams_ShouldHandleByes()
    {
        // Arrange
        var generator = new KnockoutGenerator();
        var teams = new List<Team>
        {
            new Team("Team 1"),
            new Team("Team 2"),
            new Team("Team 3")
        };
        var format = TournamentFormat.Knockout();

        // Act
        var matches = generator.GenerateMatches(teams, format).ToList();

        // Assert
        // 3 teams need to expand to 4 (next power of 2)
        // The actual non-BYE matches should be generated
        matches.Should().NotBeEmpty();
        matches.Should().AllSatisfy(m => m.TeamAId.Should().NotBeEmpty());
    }

    [Fact]
    public void GenerateMatches_ShouldHaveCorrectStages()
    {
        // Arrange
        var generator = new KnockoutGenerator();
        var teams = new List<Team>
        {
            new Team("Team 1"),
            new Team("Team 2"),
            new Team("Team 3"),
            new Team("Team 4")
        };
        var format = TournamentFormat.Knockout();

        // Act
        var matches = generator.GenerateMatches(teams, format).ToList();

        // Assert
        var stages = matches.Select(m => m.Stage).ToList();
        stages.Should().Contain(MatchStage.SemiFinal);
        stages.Should().Contain(MatchStage.Final);
    }

    [Fact]
    public void GenerateMatches_FinalShouldHavePrerequisites()
    {
        // Arrange
        var generator = new KnockoutGenerator();
        var teams = new List<Team>
        {
            new Team("Team 1"),
            new Team("Team 2"),
            new Team("Team 3"),
            new Team("Team 4")
        };
        var format = TournamentFormat.Knockout();

        // Act
        var matches = generator.GenerateMatches(teams, format).ToList();

        // Assert
        var finalMatch = matches.FirstOrDefault(m => m.Stage == MatchStage.Final);
        finalMatch.Should().NotBeNull();
        finalMatch!.PrerequisiteMatchIds.Should().HaveCount(2); // Depends on both semi-finals
    }

    [Fact]
    public void GenerateMatches_WithTwoTeams_ShouldGenerateOnlyFinal()
    {
        // Arrange
        var generator = new KnockoutGenerator();
        var teams = new List<Team>
        {
            new Team("Team 1"),
            new Team("Team 2")
        };
        var format = TournamentFormat.Knockout();

        // Act
        var matches = generator.GenerateMatches(teams, format).ToList();

        // Assert
        matches.Should().HaveCount(1);
        matches[0].Stage.Should().Be(MatchStage.Final);
    }

    [Fact]
    public void GenerateMatches_WithLessThanTwoTeams_ShouldThrowArgumentException()
    {
        // Arrange
        var generator = new KnockoutGenerator();
        var teams = new List<Team> { new Team("Team 1") };
        var format = TournamentFormat.Knockout();

        // Act
        var act = () => generator.GenerateMatches(teams, format);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Knockout tournament requires at least 2 teams*");
    }

    [Fact]
    public void GenerateMatches_ShouldNotHaveTeamPlayingItself()
    {
        // Arrange
        var generator = new KnockoutGenerator();
        var teams = new List<Team>
        {
            new Team("Team 1"),
            new Team("Team 2"),
            new Team("Team 3"),
            new Team("Team 4")
        };
        var format = TournamentFormat.Knockout();

        // Act
        var matches = generator.GenerateMatches(teams, format).ToList();

        // Assert
        matches.Should().NotContain(m => m.TeamAId == m.TeamBId);
    }

    [Fact]
    public void GenerateMatches_RoundNumbersShouldIncrementCorrectly()
    {
        // Arrange
        var generator = new KnockoutGenerator();
        var teams = new List<Team>
        {
            new Team("Team 1"),
            new Team("Team 2"),
            new Team("Team 3"),
            new Team("Team 4"),
            new Team("Team 5"),
            new Team("Team 6"),
            new Team("Team 7"),
            new Team("Team 8")
        };
        var format = TournamentFormat.Knockout();

        // Act
        var matches = generator.GenerateMatches(teams, format).ToList();

        // Assert
        var roundNumbers = matches.Select(m => m.RoundNumber).Distinct().OrderBy(r => r).ToList();
        
        // Round numbers should start at 1 and increment
        roundNumbers[0].Should().Be(1);
        
        for (int i = 1; i < roundNumbers.Count; i++)
        {
            roundNumbers[i].Should().Be(roundNumbers[i - 1] + 1);
        }
    }
}
