using FluentAssertions;
using Moq;
using ScheduleGenerator.Application.Interfaces;
using ScheduleGenerator.Application.Models;
using ScheduleGenerator.Application.Services;
using ScheduleGenerator.Domain.Constraints;
using DomainMatch = ScheduleGenerator.Domain.Entities.Match;
using DomainTournament = ScheduleGenerator.Domain.Entities.Tournament;

namespace ScheduleGenerator.Application.Tests.Services;

public class TournamentOrchestratorTests
{
    private readonly ITournamentOrchestrator _orchestrator;

    public TournamentOrchestratorTests()
    {
        var matchGenerationService = new MatchGenerationService();
        var slotGenerationService = new SlotGenerationService();
        
        // Create a mock scheduling engine
        var mockSchedulingEngine = new Mock<ISchedulingEngine>();
        mockSchedulingEngine
            .Setup(x => x.ScheduleAsync(
                It.IsAny<IEnumerable<DomainMatch>>(),
                It.IsAny<IEnumerable<Slot>>(),
                It.IsAny<IEnumerable<IConstraint>>(),
                It.IsAny<DomainTournament>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<DomainMatch> m, IEnumerable<Slot> s, IEnumerable<IConstraint> c, DomainTournament t, CancellationToken ct) =>
            {
                var schedule = new ScheduleGenerator.Domain.Entities.Schedule(t?.Id ?? Guid.NewGuid());
                return schedule;
            });

        var schedulingService = new SchedulingService(mockSchedulingEngine.Object);

        _orchestrator = new TournamentOrchestrator(
            matchGenerationService,
            slotGenerationService,
            schedulingService);
    }

    [Fact]
    public async Task GenerateScheduleAsync_InvalidDefinition_ThrowsValidationException()
    {
        // Arrange
        var definition = new TournamentDefinition
        {
            Name = "", // Invalid - empty name
            Teams = new List<TeamDefinition>(),
            Fields = new List<FieldDefinition>(),
            Format = new FormatConfiguration { Type = "RoundRobin" },
            Rules = new SchedulingRules
            {
                MatchDurationMinutes = 90,
                BufferBetweenMatchesMinutes = 15
            }
        };

        // Act & Assert
        await Assert.ThrowsAsync<FluentValidation.ValidationException>(
            async () => await _orchestrator.GenerateScheduleAsync(definition));
    }

    [Fact]
    public async Task GenerateScheduleAsync_ValidDefinition_CreatesMatchesAndSlots()
    {
        // Arrange
        var definition = new TournamentDefinition
        {
            Name = "Test Tournament",
            Teams = new List<TeamDefinition>
            {
                new() { Name = "Team A" },
                new() { Name = "Team B" },
                new() { Name = "Team C" },
                new() { Name = "Team D" }
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
            Format = new FormatConfiguration { Type = "RoundRobin" },
            Rules = new SchedulingRules
            {
                MatchDurationMinutes = 90,
                BufferBetweenMatchesMinutes = 15
            }
        };

        // Act
        var result = await _orchestrator.GenerateScheduleAsync(definition);

        // Assert
        result.Should().NotBeNull();
        result.TournamentName.Should().Be("Test Tournament");
    }

    [Fact]
    public async Task GenerateScheduleAsync_NullDefinition_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await _orchestrator.GenerateScheduleAsync(null!));
    }
}
