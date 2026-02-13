using FluentAssertions;
using ScheduleGenerator.Application.Models;
using ScheduleGenerator.Application.Services;
using ScheduleGenerator.Domain.Entities;
using ScheduleGenerator.Domain.ValueObjects;

namespace ScheduleGenerator.Application.Tests.Services;

public class SlotGenerationServiceTests
{
    private readonly ISlotGenerationService _service;

    public SlotGenerationServiceTests()
    {
        _service = new SlotGenerationService();
    }

    [Fact]
    public void GenerateSlots_SingleFieldSingleWindow_GeneratesCorrectSlots()
    {
        // Arrange
        var field = new Field("Field 1");
        field.AddAvailabilityWindow(new TimeSlot(
            DateTime.Parse("2026-06-01T09:00:00"),
            DateTime.Parse("2026-06-01T12:00:00")
        ));
        var fields = new List<Field> { field };

        var rules = new SchedulingRules
        {
            MatchDurationMinutes = 60,
            BufferBetweenMatchesMinutes = 15
        };

        // Act
        var slots = _service.GenerateSlots(fields, rules);

        // Assert
        // 9:00-10:00, 10:15-11:15 (can't fit another after 11:30)
        slots.Should().HaveCount(2);
        slots[0].StartTime.Should().Be(DateTime.Parse("2026-06-01T09:00:00"));
        slots[0].EndTime.Should().Be(DateTime.Parse("2026-06-01T10:00:00"));
        slots[1].StartTime.Should().Be(DateTime.Parse("2026-06-01T10:15:00"));
        slots[1].EndTime.Should().Be(DateTime.Parse("2026-06-01T11:15:00"));
    }

    [Fact]
    public void GenerateSlots_MultipleFields_GeneratesSlotsForEach()
    {
        // Arrange
        var field1 = new Field("Field 1");
        field1.AddAvailabilityWindow(new TimeSlot(
            DateTime.Parse("2026-06-01T09:00:00"),
            DateTime.Parse("2026-06-01T10:00:00")
        ));

        var field2 = new Field("Field 2");
        field2.AddAvailabilityWindow(new TimeSlot(
            DateTime.Parse("2026-06-01T09:00:00"),
            DateTime.Parse("2026-06-01T10:00:00")
        ));

        var fields = new List<Field> { field1, field2 };

        var rules = new SchedulingRules
        {
            MatchDurationMinutes = 30,
            BufferBetweenMatchesMinutes = 15
        };

        // Act
        var slots = _service.GenerateSlots(fields, rules);

        // Assert
        // Each field should have 1 slot (9:00-9:30, next would start at 9:45, can't fit)
        slots.Should().HaveCount(2);
        slots.Should().ContainSingle(s => s.Field.Name == "Field 1");
        slots.Should().ContainSingle(s => s.Field.Name == "Field 2");
    }

    [Fact]
    public void GenerateSlots_NoBuffer_GeneratesConsecutiveSlots()
    {
        // Arrange
        var field = new Field("Field 1");
        field.AddAvailabilityWindow(new TimeSlot(
            DateTime.Parse("2026-06-01T09:00:00"),
            DateTime.Parse("2026-06-01T11:00:00")
        ));
        var fields = new List<Field> { field };

        var rules = new SchedulingRules
        {
            MatchDurationMinutes = 30,
            BufferBetweenMatchesMinutes = 0
        };

        // Act
        var slots = _service.GenerateSlots(fields, rules);

        // Assert
        // 9:00-9:30, 9:30-10:00, 10:00-10:30, 10:30-11:00
        slots.Should().HaveCount(4);
        slots[0].EndTime.Should().Be(slots[1].StartTime);
        slots[1].EndTime.Should().Be(slots[2].StartTime);
    }

    [Fact]
    public void GenerateSlots_MultipleWindowsSameField_GeneratesSlotsForEachWindow()
    {
        // Arrange
        var field = new Field("Field 1");
        field.AddAvailabilityWindow(new TimeSlot(
            DateTime.Parse("2026-06-01T09:00:00"),
            DateTime.Parse("2026-06-01T10:00:00")
        ));
        field.AddAvailabilityWindow(new TimeSlot(
            DateTime.Parse("2026-06-01T14:00:00"),
            DateTime.Parse("2026-06-01T15:00:00")
        ));
        var fields = new List<Field> { field };

        var rules = new SchedulingRules
        {
            MatchDurationMinutes = 45,
            BufferBetweenMatchesMinutes = 0
        };

        // Act
        var slots = _service.GenerateSlots(fields, rules);

        // Assert
        // One slot in each window
        slots.Should().HaveCount(2);
        slots[0].StartTime.Should().Be(DateTime.Parse("2026-06-01T09:00:00"));
        slots[1].StartTime.Should().Be(DateTime.Parse("2026-06-01T14:00:00"));
    }

    [Fact]
    public void GenerateSlots_WindowTooSmall_GeneratesNoSlots()
    {
        // Arrange
        var field = new Field("Field 1");
        field.AddAvailabilityWindow(new TimeSlot(
            DateTime.Parse("2026-06-01T09:00:00"),
            DateTime.Parse("2026-06-01T09:30:00")
        ));
        var fields = new List<Field> { field };

        var rules = new SchedulingRules
        {
            MatchDurationMinutes = 60,
            BufferBetweenMatchesMinutes = 0
        };

        // Act
        var slots = _service.GenerateSlots(fields, rules);

        // Assert
        slots.Should().BeEmpty();
    }

    [Fact]
    public void GenerateSlots_NoFields_ReturnsEmptyList()
    {
        // Arrange
        var fields = new List<Field>();
        var rules = new SchedulingRules
        {
            MatchDurationMinutes = 60,
            BufferBetweenMatchesMinutes = 15
        };

        // Act
        var slots = _service.GenerateSlots(fields, rules);

        // Assert
        slots.Should().BeEmpty();
    }
}
