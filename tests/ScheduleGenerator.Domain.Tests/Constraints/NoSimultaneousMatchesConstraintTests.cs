using FluentAssertions;
using ScheduleGenerator.Domain.Constraints;
using ScheduleGenerator.Domain.Entities;
using ScheduleGenerator.Domain.ValueObjects;

namespace ScheduleGenerator.Domain.Tests.Constraints;

public class NoSimultaneousMatchesConstraintTests
{
    [Fact]
    public void Evaluate_WithNoOverlappingMatches_ShouldReturnNoViolations()
    {
        // Arrange
        var constraint = new NoSimultaneousMatchesConstraint();
        
        var team1 = new Team("Team 1");
        var team2 = new Team("Team 2");
        var team3 = new Team("Team 3");
        var team4 = new Team("Team 4");
        
        var tournament = CreateTournament(team1, team2, team3, team4);
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
            new TimeSlot(new DateTime(2026, 2, 13, 12, 0, 0), new DateTime(2026, 2, 13, 13, 30, 0)));

        schedule.AddMatch(match1);
        schedule.AddMatch(match2);

        // Act
        var violations = constraint.Evaluate(schedule, tournament);

        // Assert
        violations.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_WithOverlappingMatches_ShouldReturnViolation()
    {
        // Arrange
        var constraint = new NoSimultaneousMatchesConstraint();
        
        var team1 = new Team("Team 1");
        var team2 = new Team("Team 2");
        var team3 = new Team("Team 3");
        
        var tournament = CreateTournament(team1, team2, team3);
        var schedule = new Schedule(tournament.Id);

        var match1 = new Match(team1.Id, team2.Id, MatchStage.RoundRobin, 1);
        match1.AssignSlot(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new TimeSlot(new DateTime(2026, 2, 13, 10, 0, 0), new DateTime(2026, 2, 13, 12, 0, 0)));

        var match2 = new Match(team1.Id, team3.Id, MatchStage.RoundRobin, 2);
        match2.AssignSlot(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new TimeSlot(new DateTime(2026, 2, 13, 11, 0, 0), new DateTime(2026, 2, 13, 13, 0, 0)));

        schedule.AddMatch(match1);
        schedule.AddMatch(match2);

        // Act
        var violations = constraint.Evaluate(schedule, tournament).ToList();

        // Assert
        violations.Should().HaveCount(1);
        violations[0].ConstraintName.Should().Be("No Simultaneous Matches");
        violations[0].Severity.Should().Be(ConstraintSeverity.Critical);
        violations[0].Description.Should().Contain("Team 1");
        violations[0].Description.Should().Contain("overlapping");
    }

    [Fact]
    public void Evaluate_WithUnscheduledMatches_ShouldIgnoreThem()
    {
        // Arrange
        var constraint = new NoSimultaneousMatchesConstraint();
        
        var team1 = new Team("Team 1");
        var team2 = new Team("Team 2");
        
        var tournament = CreateTournament(team1, team2);
        var schedule = new Schedule(tournament.Id);

        var match1 = new Match(team1.Id, team2.Id, MatchStage.RoundRobin, 1);
        // Not assigned - unscheduled

        schedule.AddMatch(match1);

        // Act
        var violations = constraint.Evaluate(schedule, tournament);

        // Assert
        violations.Should().BeEmpty();
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
