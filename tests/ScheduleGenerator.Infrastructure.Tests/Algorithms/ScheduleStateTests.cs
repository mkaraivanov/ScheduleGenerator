using FluentAssertions;
using ScheduleGenerator.Application.Models;
using ScheduleGenerator.Domain.Entities;
using ScheduleGenerator.Domain.ValueObjects;
using ScheduleGenerator.Infrastructure.Algorithms;
using Xunit;

namespace ScheduleGenerator.Infrastructure.Tests.Algorithms;

public class ScheduleStateTests
{
    [Fact]
    public void Constructor_InitializesEmptyState()
    {
        // Arrange
        var matches = CreateSampleMatches(3);
        var slots = CreateSampleSlots(5);

        // Act
        var state = new ScheduleState(matches, slots);

        // Assert
        state.AssignedCount.Should().Be(0);
        state.UsedSlotIds.Should().BeEmpty();
    }

    [Fact]
    public void AssignMatch_AddsMatchToState()
    {
        // Arrange
        var matches = CreateSampleMatches(3);
        var slots = CreateSampleSlots(5);
        var state = new ScheduleState(matches, slots);
        var match = matches.First();
        var slot = slots.First();
        var slotId = Guid.NewGuid();

        // Act
        state.AssignMatch(match, slot, slotId);

        // Assert
        state.AssignedCount.Should().Be(1);
        state.IsSlotUsed(slotId).Should().BeTrue();
        state.MatchAssignments.Should().ContainKey(match.Id);
    }

    [Fact]
    public void UnassignMatch_RemovesMatchFromState()
    {
        // Arrange
        var matches = CreateSampleMatches(3);
        var slots = CreateSampleSlots(5);
        var state = new ScheduleState(matches, slots);
        var match = matches.First();
        var slot = slots.First();
        var slotId = Guid.NewGuid();

        state.AssignMatch(match, slot, slotId);

        // Act
        state.UnassignMatch(match, slotId);

        // Assert
        state.AssignedCount.Should().Be(0);
        state.IsSlotUsed(slotId).Should().BeFalse();
        state.MatchAssignments.Should().NotContainKey(match.Id);
    }

    [Fact]
    public void GetTeamSchedule_ReturnsAssignedSlots()
    {
        // Arrange
        var matches = CreateSampleMatches(3);
        var slots = CreateSampleSlots(5);
        var state = new ScheduleState(matches, slots);
        var match = matches.First();
        var slot = slots.First();
        var slotId = Guid.NewGuid();

        // Act
        state.AssignMatch(match, slot, slotId);
        var teamASchedule = state.GetTeamSchedule(match.TeamAId);
        var teamBSchedule = state.GetTeamSchedule(match.TeamBId);

        // Assert
        teamASchedule.Should().Contain(slot);
        teamBSchedule.Should().Contain(slot);
    }

    [Fact]
    public void RemoveFeasibleSlot_RemovesSlotFromFeasibleSet()
    {
        // Arrange
        var matches = CreateSampleMatches(3);
        var slots = CreateSampleSlots(5);
        var state = new ScheduleState(matches, slots);
        var match = matches.First();
        var slotId = Guid.NewGuid();

        // Act
        state.RemoveFeasibleSlot(match.Id, slotId);
        var feasibleSlots = state.GetFeasibleSlots(match.Id);

        // Assert
        feasibleSlots.Should().NotContain(slotId);
    }

    private List<Match> CreateSampleMatches(int count)
    {
        var matches = new List<Match>();
        for (int i = 0; i < count; i++)
        {
            matches.Add(new Match(
                Guid.NewGuid(),
                Guid.NewGuid(),
                MatchStage.RoundRobin,
                1));
        }
        return matches;
    }

    private List<Slot> CreateSampleSlots(int count)
    {
        var field = new Field("Field 1");
        var slots = new List<Slot>();
        var startTime = DateTime.Today.AddHours(9);

        for (int i = 0; i < count; i++)
        {
            slots.Add(new Slot
            {
                Field = field,
                StartTime = startTime.AddHours(i),
                EndTime = startTime.AddHours(i + 1)
            });
        }

        return slots;
    }
}
