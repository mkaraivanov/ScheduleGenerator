using FluentAssertions;
using ScheduleGenerator.Application.Models;
using ScheduleGenerator.Domain.Entities;

namespace ScheduleGenerator.Application.Tests.Models;

public class SlotTests
{
    [Fact]
    public void Slot_OverlapsWith_SameFieldAndTime_ReturnsTrue()
    {
        // Arrange
        var field = new Field("Field 1");
        var slot1 = new Slot
        {
            Field = field,
            StartTime = DateTime.Parse("2026-06-01T10:00:00"),
            EndTime = DateTime.Parse("2026-06-01T11:00:00")
        };
        var slot2 = new Slot
        {
            Field = field,
            StartTime = DateTime.Parse("2026-06-01T10:30:00"),
            EndTime = DateTime.Parse("2026-06-01T11:30:00")
        };

        // Act
        var overlaps = slot1.OverlapsWith(slot2);

        // Assert
        overlaps.Should().BeTrue();
    }

    [Fact]
    public void Slot_OverlapsWith_SameFieldNoTimeOverlap_ReturnsFalse()
    {
        // Arrange
        var field = new Field("Field 1");
        var slot1 = new Slot
        {
            Field = field,
            StartTime = DateTime.Parse("2026-06-01T10:00:00"),
            EndTime = DateTime.Parse("2026-06-01T11:00:00")
        };
        var slot2 = new Slot
        {
            Field = field,
            StartTime = DateTime.Parse("2026-06-01T11:00:00"),
            EndTime = DateTime.Parse("2026-06-01T12:00:00")
        };

        // Act
        var overlaps = slot1.OverlapsWith(slot2);

        // Assert
        overlaps.Should().BeFalse();
    }

    [Fact]
    public void Slot_OverlapsWith_DifferentFields_ReturnsFalse()
    {
        // Arrange
        var field1 = new Field("Field 1");
        var field2 = new Field("Field 2");
        var slot1 = new Slot
        {
            Field = field1,
            StartTime = DateTime.Parse("2026-06-01T10:00:00"),
            EndTime = DateTime.Parse("2026-06-01T11:00:00")
        };
        var slot2 = new Slot
        {
            Field = field2,
            StartTime = DateTime.Parse("2026-06-01T10:00:00"),
            EndTime = DateTime.Parse("2026-06-01T11:00:00")
        };

        // Act
        var overlaps = slot1.OverlapsWith(slot2);

        // Assert
        overlaps.Should().BeFalse();
    }
}
