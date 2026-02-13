using FluentAssertions;
using ScheduleGenerator.Domain.Constraints;
using ScheduleGenerator.Domain.Entities;
using ScheduleGenerator.Domain.ValueObjects;

namespace ScheduleGenerator.Domain.Tests.Constraints;

public class MinimumRestTimeConstraintTests
{
    [Fact]
    public void Evaluate_WithSufficientRestTime_ShouldReturnNoViolations()
    {
        // Arrange
        var minimumRest = TimeSpan.FromHours(2);
        var constraint = new MinimumRestTimeConstraint(minimumRest);
        
        var team1 = new Team("Team 1");
        var team2 = new Team("Team 2");
        var team3 = new Team("Team 3");
        
        var tournament = CreateTournament(team1, team2, team3);
        var schedule = new Schedule(tournament.Id);

        var match1 = new Match(team1.Id, team2.Id, MatchStage.RoundRobin, 1);
        match1.AssignSlot(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new TimeSlot(new DateTime(2026, 2, 13, 10, 0, 0), new DateTime(2026, 2, 13, 11, 30, 0)));

        var match2 = new Match(team1.Id, team3.Id, MatchStage.RoundRobin, 2);
        match2.AssignSlot(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new TimeSlot(new DateTime(2026, 2, 13, 14, 0, 0), new DateTime(2026, 2, 13, 15, 30, 0)));

        schedule.AddMatch(match1);
        schedule.AddMatch(match2);

        // Act
        var violations = constraint.Evaluate(schedule, tournament);

        // Assert
        violations.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_WithInsufficientRestTime_ShouldReturnViolation()
    {
        // Arrange
        var minimumRest = TimeSpan.FromHours(3);
        var constraint = new MinimumRestTimeConstraint(minimumRest);
        
        var team1 = new Team("Team 1");
        var team2 = new Team("Team 2");
        var team3 = new Team("Team 3");
        
        var tournament = CreateTournament(team1, team2, team3);
        var schedule = new Schedule(tournament.Id);

        var match1 = new Match(team1.Id, team2.Id, MatchStage.RoundRobin, 1);
        match1.AssignSlot(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new TimeSlot(new DateTime(2026, 2, 13, 10, 0, 0), new DateTime(2026, 2, 13, 11, 30, 0)));

        var match2 = new Match(team1.Id, team3.Id, MatchStage.RoundRobin, 2);
        match2.AssignSlot(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new TimeSlot(new DateTime(2026, 2, 13, 13, 0, 0), new DateTime(2026, 2, 13, 14, 30, 0)));

        schedule.AddMatch(match1);
        schedule.AddMatch(match2);

        // Act
        var violations = constraint.Evaluate(schedule, tournament).ToList();

        // Assert
        violations.Should().HaveCount(1);
        violations[0].ConstraintName.Should().Be("Minimum Rest Time");
        violations[0].Severity.Should().Be(ConstraintSeverity.Critical);
        violations[0].Description.Should().Contain("Team 1");
        violations[0].Description.Should().Contain("insufficient rest time");
    }

    [Fact]
    public void Evaluate_WithMultipleViolations_ShouldReturnAllViolations()
    {
        // Arrange
        var minimumRest = TimeSpan.FromHours(4);
        var constraint = new MinimumRestTimeConstraint(minimumRest);
        
        var team1 = new Team("Team 1");
        var team2 = new Team("Team 2");
        var team3 = new Team("Team 3");
        var team4 = new Team("Team 4");
        
        var tournament = CreateTournament(team1, team2, team3, team4);
        var schedule = new Schedule(tournament.Id);

        // Team 1 matches with insufficient rest
        var match1 = new Match(team1.Id, team2.Id, MatchStage.RoundRobin, 1);
        match1.AssignSlot(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new TimeSlot(new DateTime(2026, 2, 13, 10, 0, 0), new DateTime(2026, 2, 13, 11, 30, 0)));

        var match2 = new Match(team1.Id, team3.Id, MatchStage.RoundRobin, 2);
        match2.AssignSlot(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new TimeSlot(new DateTime(2026, 2, 13, 13, 0, 0), new DateTime(2026, 2, 13, 14, 30, 0)));

        var match3 = new Match(team1.Id, team4.Id, MatchStage.RoundRobin, 3);
        match3.AssignSlot(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new TimeSlot(new DateTime(2026, 2, 13, 16, 0, 0), new DateTime(2026, 2, 13, 17, 30, 0)));

        schedule.AddMatch(match1);
        schedule.AddMatch(match2);
        schedule.AddMatch(match3);

        // Act
        var violations = constraint.Evaluate(schedule, tournament).ToList();

        // Assert
        violations.Should().HaveCount(2); // Two pairs of consecutive matches violate the constraint
    }

    [Fact]
    public void Constructor_WithNegativeRestTime_ShouldThrowArgumentException()
    {
        // Act
        var act = () => new MinimumRestTimeConstraint(TimeSpan.FromHours(-1));

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Minimum rest time cannot be negative*");
    }

    private Tournament CreateTournament(params Team[] teams)
    {
        var tournament = new Tournament(
            "Test Tournament",
            TournamentFormat.RoundRobin(),
            MatchDuration.Standard);

        foreach (var team in teams)
        {
            tournament.AddTeam(team);
        }

        return tournament;
    }
}
