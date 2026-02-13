using FluentAssertions;
using ScheduleGenerator.Domain.Entities;
using ScheduleGenerator.Domain.Formats;

namespace ScheduleGenerator.Domain.Tests.Formats;

public class GroupStageGeneratorTests
{
    [Fact]
    public void GenerateMatches_WithValidTeamsAndGroups_ShouldGenerateMatches()
    {
        // Arrange
        var generator = new GroupStageGenerator();
        var teams = new List<Team>
        {
            new Team("Team 1"),
            new Team("Team 2"),
            new Team("Team 3"),
            new Team("Team 4"),
            new Team("Team 5"),
            new Team("Team 6")
        };
        var format = TournamentFormat.Groups(groupCount: 2, teamsAdvancingPerGroup: 2);

        // Act
        var matches = generator.GenerateMatches(teams, format).ToList();

        // Assert
        matches.Should().NotBeEmpty();
    }

    [Fact]
    public void GenerateMatches_ShouldDistributeTeamsIntoCorrectNumberOfGroups()
    {
        // Arrange
        var generator = new GroupStageGenerator();
        var teams = new List<Team>
        {
            new Team("Team 1"),
            new Team("Team 2"),
            new Team("Team 3"),
            new Team("Team 4")
        };
        var format = TournamentFormat.Groups(groupCount: 2, teamsAdvancingPerGroup: 1);

        // Act
        var matches = generator.GenerateMatches(teams, format).ToList();

        // Assert
        var groups = matches.Select(m => m.GroupIdentifier).Distinct().ToList();
        groups.Should().HaveCount(2);
    }

    [Fact]
    public void GenerateMatches_AllMatchesShouldHaveGroupStage()
    {
        // Arrange
        var generator = new GroupStageGenerator();
        var teams = new List<Team>
        {
            new Team("Team 1"),
            new Team("Team 2"),
            new Team("Team 3"),
            new Team("Team 4")
        };
        var format = TournamentFormat.Groups(groupCount: 2, teamsAdvancingPerGroup: 1);

        // Act
        var matches = generator.GenerateMatches(teams, format).ToList();

        // Assert
        matches.Should().AllSatisfy(m => m.Stage.Should().Be(MatchStage.GroupStage));
    }

    [Fact]
    public void GenerateMatches_AllMatchesShouldHaveGroupIdentifier()
    {
        // Arrange
        var generator = new GroupStageGenerator();
        var teams = new List<Team>
        {
            new Team("Team 1"),
            new Team("Team 2"),
            new Team("Team 3"),
            new Team("Team 4")
        };
        var format = TournamentFormat.Groups(groupCount: 2, teamsAdvancingPerGroup: 1);

        // Act
        var matches = generator.GenerateMatches(teams, format).ToList();

        // Assert
        matches.Should().AllSatisfy(m => m.GroupIdentifier.Should().NotBeNullOrEmpty());
    }

    [Fact]
    public void GenerateMatches_WithSeededTeams_ShouldDistributeSeededTeamsAcrossGroups()
    {
        // Arrange
        var generator = new GroupStageGenerator();
        var teams = new List<Team>
        {
            new Team("Team 1", seed: 1),
            new Team("Team 2", seed: 2),
            new Team("Team 3", seed: 3),
            new Team("Team 4", seed: 4)
        };
        var format = TournamentFormat.Groups(groupCount: 2, teamsAdvancingPerGroup: 1);

        // Act
        var matches = generator.GenerateMatches(teams, format).ToList();

        // Assert
        var groups = matches.GroupBy(m => m.GroupIdentifier).ToList();
        
        // Both groups should have matches
        groups.Should().HaveCount(2);
        
        // Each group should have teams
        foreach (var group in groups)
        {
            group.Should().NotBeEmpty();
        }
    }

    [Fact]
    public void GenerateMatches_WithInsufficientTeams_ShouldThrowArgumentException()
    {
        // Arrange
        var generator = new GroupStageGenerator();
        var teams = new List<Team>
        {
            new Team("Team 1"),
            new Team("Team 2")
        };
        var format = TournamentFormat.Groups(groupCount: 3, teamsAdvancingPerGroup: 1);

        // Act
        var act = () => generator.GenerateMatches(teams, format);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Not enough teams for*");
    }

    [Fact]
    public void GenerateMatches_WithoutGroupCount_ShouldThrowArgumentException()
    {
        // Arrange
        var generator = new GroupStageGenerator();
        var teams = new List<Team>
        {
            new Team("Team 1"),
            new Team("Team 2"),
            new Team("Team 3"),
            new Team("Team 4")
        };
        var format = new TournamentFormat(TournamentType.GroupStage); // No group count

        // Act
        var act = () => generator.GenerateMatches(teams, format);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GenerateMatches_EachGroupShouldHaveRoundRobinMatches()
    {
        // Arrange
        var generator = new GroupStageGenerator();
        var teams = new List<Team>
        {
            new Team("Team 1"),
            new Team("Team 2"),
            new Team("Team 3"),
            new Team("Team 4")
        };
        var format = TournamentFormat.Groups(groupCount: 2, teamsAdvancingPerGroup: 1);

        // Act
        var matches = generator.GenerateMatches(teams, format).ToList();

        // Assert
        var groupMatches = matches.GroupBy(m => m.GroupIdentifier).ToList();
        
        foreach (var group in groupMatches)
        {
            var teamsInGroup = group
                .SelectMany(m => new[] { m.TeamAId, m.TeamBId })
                .Distinct()
                .ToList();

            // Each team in a group should play every other team in that group
            foreach (var teamId in teamsInGroup)
            {
                var opponentsForTeam = group
                    .Where(m => m.TeamAId == teamId || m.TeamBId == teamId)
                    .SelectMany(m => new[] { m.TeamAId, m.TeamBId })
                    .Where(id => id != teamId)
                    .Distinct()
                    .ToList();

                opponentsForTeam.Should().HaveCount(teamsInGroup.Count - 1);
            }
        }
    }
}
