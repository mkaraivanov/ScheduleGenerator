using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using ScheduleGenerator.Application.Models;
using ScheduleGenerator.Domain.Constraints;
using ScheduleGenerator.Domain.Entities;
using ScheduleGenerator.Domain.ValueObjects;
using ScheduleGenerator.Infrastructure.Algorithms;
using Xunit;

namespace ScheduleGenerator.Infrastructure.Tests.Algorithms;

public class BacktrackingSchedulerTests
{
    [Fact]
    public async Task ScheduleAsync_WithSolvableScenario_ReturnsCompleteSchedule()
    {
        // Arrange
        var scheduler = new BacktrackingScheduler(NullLogger<BacktrackingScheduler>.Instance);
        var tournament = CreateTournament(3);
        var matches = CreateSimpleMatches(tournament, 3); // 3 matches
        var slots = CreateSimpleSlots(3);     // 3 slots
        var constraints = CreateBasicConstraints();

        // Act
        var schedule = await scheduler.ScheduleAsync(matches, slots, constraints, tournament);

        // Assert
        schedule.Should().NotBeNull();
        schedule.ScheduledMatchCount.Should().Be(3);
        schedule.IsFeasible.Should().BeTrue();
    }

    [Fact]
    public async Task ScheduleAsync_WithInsufficientSlots_MarksAsInfeasible()
    {
        // Arrange
        var scheduler = new BacktrackingScheduler(NullLogger<BacktrackingScheduler>.Instance);
        var tournament = CreateTournament(5);
        var matches = CreateSimpleMatches(tournament, 5); // 5 matches
        var slots = CreateSimpleSlots(3);     // Only 3 slots
        var constraints = CreateBasicConstraints();

        // Act
        var schedule = await scheduler.ScheduleAsync(matches, slots, constraints, tournament);

        // Assert
        schedule.Should().NotBeNull();
        schedule.IsFeasible.Should().BeFalse();
    }

    [Fact]
    public async Task ScheduleAsync_WithConflictingTeams_RespectsConstraints()
    {
        // Arrange
        var scheduler = new BacktrackingScheduler(NullLogger<BacktrackingScheduler>.Instance);
        var tournament = CreateTournament(3);
        var teams = tournament.Teams.ToList();
        
        // Create matches where team 0 plays in two matches
        var matches = new List<Match>
        {
            new Match(teams[0].Id, teams[1].Id, MatchStage.RoundRobin, 1),
            new Match(teams[0].Id, teams[2].Id, MatchStage.RoundRobin, 1)
        };

        // Create overlapping slots
        var field = new Field("Field 1");
        var baseTime = DateTime.Today.AddHours(10);
        var slots = new List<Slot>
        {
            new Slot { Field = field, StartTime = baseTime, EndTime = baseTime.AddHours(1) },
            new Slot { Field = field, StartTime = baseTime.AddMinutes(30), EndTime = baseTime.AddHours(1.5) }
        };

        var constraints = new List<IConstraint>
        {
            new NoSimultaneousMatchesConstraint()
        };

        // Act
        var schedule = await scheduler.ScheduleAsync(matches, slots, constraints, tournament);

        // Assert
        schedule.Should().NotBeNull();
        // The scheduler should not assign both matches to overlapping slots
        if (schedule.ScheduledMatchCount == 2)
        {
            var match1 = schedule.ScheduledMatches.First();
            var match2 = schedule.ScheduledMatches.Last();
            
            // Ensure they don't overlap if team is in both
            var timeOverlaps = match1.AssignedTimeSlot!.OverlapsWith(match2.AssignedTimeSlot!);
            timeOverlaps.Should().BeFalse();
        }
    }

    [Fact]
    public async Task ScheduleAsync_WithEmptyMatchList_ReturnsEmptySchedule()
    {
        // Arrange
        var scheduler = new BacktrackingScheduler(NullLogger<BacktrackingScheduler>.Instance);
        var tournament = CreateTournament(0);
        var matches = new List<Match>();
        var slots = CreateSimpleSlots(3);
        var constraints = CreateBasicConstraints();

        // Act
        var schedule = await scheduler.ScheduleAsync(matches, slots, constraints, tournament);

        // Assert
        schedule.Should().NotBeNull();
        schedule.ScheduledMatchCount.Should().Be(0);
        schedule.IsFeasible.Should().BeTrue();
    }

    [Fact]
    public async Task ScheduleAsync_WithCancellation_StopsExecution()
    {
        // Arrange
        var scheduler = new BacktrackingScheduler(NullLogger<BacktrackingScheduler>.Instance);
        var tournament = CreateTournament(10);
        var matches = CreateSimpleMatches(tournament, 10);
        var slots = CreateSimpleSlots(10);
        var constraints = CreateBasicConstraints();
        var cts = new CancellationTokenSource();

        cts.Cancel(); // Cancel immediately

        // Act
        var schedule = await scheduler.ScheduleAsync(matches, slots, constraints, tournament, cts.Token);

        // Assert
        schedule.Should().NotBeNull();
        schedule.IsFeasible.Should().BeFalse();
    }

    private Tournament CreateTournament(int teamCount)
    {
        var tournament = new Tournament(
            "Test Tournament",
            TournamentFormat.RoundRobin,
            new MatchDuration(60, 10),
            TimeSpan.FromMinutes(30));

        for (int i = 0; i < teamCount; i++)
        {
            tournament.AddTeam(new Team($"Team {i + 1}", null, null));
        }

        return tournament;
    }

    private List<Match> CreateSimpleMatches(Tournament tournament, int count)
    {
        var matches = new List<Match>();
        var teams = tournament.Teams.ToList();

        if (teams.Count < count * 2)
        {
            // Not enough teams, create pairs
            for (int i = 0; i < count && i * 2 + 1 < teams.Count; i++)
            {
                matches.Add(new Match(
                    teams[i * 2].Id,
                    teams[i * 2 + 1].Id,
                    MatchStage.RoundRobin,
                    1));
            }
        }
        else
        {
            for (int i = 0; i < count; i++)
            {
                matches.Add(new Match(
                    teams[i * 2].Id,
                    teams[i * 2 + 1].Id,
                    MatchStage.RoundRobin,
                    1));
            }
        }

        return matches;
    }

    private List<Slot> CreateSimpleSlots(int count)
    {
        var field = new Field("Field 1");
        var slots = new List<Slot>();
        var startTime = DateTime.Today.AddHours(9);

        for (int i = 0; i < count; i++)
        {
            slots.Add(new Slot
            {
                Field = field,
                StartTime = startTime.AddHours(i * 2), // 2 hour gaps
                EndTime = startTime.AddHours(i * 2 + 1)
            });
        }

        return slots;
    }

    private List<IConstraint> CreateBasicConstraints()
    {
        return new List<IConstraint>
        {
            new NoSimultaneousMatchesConstraint()
        };
    }
}
