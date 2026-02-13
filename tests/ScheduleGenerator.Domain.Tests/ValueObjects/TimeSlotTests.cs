using FluentAssertions;
using ScheduleGenerator.Domain.ValueObjects;

namespace ScheduleGenerator.Domain.Tests.ValueObjects;

public class TimeSlotTests
{
    [Fact]
    public void Constructor_WithValidDates_ShouldCreateTimeSlot()
    {
        // Arrange
        var start = new DateTime(2026, 2, 13, 10, 0, 0);
        var end = new DateTime(2026, 2, 13, 12, 0, 0);

        // Act
        var timeSlot = new TimeSlot(start, end);

        // Assert
        timeSlot.Start.Should().Be(start);
        timeSlot.End.Should().Be(end);
    }

    [Fact]
    public void Constructor_WithEndBeforeStart_ShouldThrowArgumentException()
    {
        // Arrange
        var start = new DateTime(2026, 2, 13, 12, 0, 0);
        var end = new DateTime(2026, 2, 13, 10, 0, 0);

        // Act
        var act = () => new TimeSlot(start, end);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*End time must be after start time*");
    }

    [Fact]
    public void Constructor_WithEqualStartAndEnd_ShouldThrowArgumentException()
    {
        // Arrange
        var dateTime = new DateTime(2026, 2, 13, 10, 0, 0);

        // Act
        var act = () => new TimeSlot(dateTime, dateTime);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Duration_ShouldReturnCorrectTimeSpan()
    {
        // Arrange
        var start = new DateTime(2026, 2, 13, 10, 0, 0);
        var end = new DateTime(2026, 2, 13, 12, 30, 0);
        var timeSlot = new TimeSlot(start, end);

        // Act
        var duration = timeSlot.Duration;

        // Assert
        duration.Should().Be(TimeSpan.FromMinutes(150));
    }

    [Fact]
    public void OverlapsWith_WithOverlappingSlots_ShouldReturnTrue()
    {
        // Arrange
        var slot1 = new TimeSlot(
            new DateTime(2026, 2, 13, 10, 0, 0),
            new DateTime(2026, 2, 13, 12, 0, 0));
        var slot2 = new TimeSlot(
            new DateTime(2026, 2, 13, 11, 0, 0),
            new DateTime(2026, 2, 13, 13, 0, 0));

        // Act
        var overlaps = slot1.OverlapsWith(slot2);

        // Assert
        overlaps.Should().BeTrue();
    }

    [Fact]
    public void OverlapsWith_WithNonOverlappingSlots_ShouldReturnFalse()
    {
        // Arrange
        var slot1 = new TimeSlot(
            new DateTime(2026, 2, 13, 10, 0, 0),
            new DateTime(2026, 2, 13, 12, 0, 0));
        var slot2 = new TimeSlot(
            new DateTime(2026, 2, 13, 12, 0, 0),
            new DateTime(2026, 2, 13, 14, 0, 0));

        // Act
        var overlaps = slot1.OverlapsWith(slot2);

        // Assert
        overlaps.Should().BeFalse();
    }

    [Fact]
    public void Contains_WithDateTimeInSlot_ShouldReturnTrue()
    {
        // Arrange
        var timeSlot = new TimeSlot(
            new DateTime(2026, 2, 13, 10, 0, 0),
            new DateTime(2026, 2, 13, 12, 0, 0));
        var dateTime = new DateTime(2026, 2, 13, 11, 0, 0);

        // Act
        var contains = timeSlot.Contains(dateTime);

        // Assert
        contains.Should().BeTrue();
    }

    [Fact]
    public void Contains_WithDateTimeOutsideSlot_ShouldReturnFalse()
    {
        // Arrange
        var timeSlot = new TimeSlot(
            new DateTime(2026, 2, 13, 10, 0, 0),
            new DateTime(2026, 2, 13, 12, 0, 0));
        var dateTime = new DateTime(2026, 2, 13, 13, 0, 0);

        // Act
        var contains = timeSlot.Contains(dateTime);

        // Assert
        contains.Should().BeFalse();
    }

    [Fact]
    public void TimeBetween_WithNonOverlappingSlots_ShouldReturnCorrectInterval()
    {
        // Arrange
        var slot1 = new TimeSlot(
            new DateTime(2026, 2, 13, 10, 0, 0),
            new DateTime(2026, 2, 13, 12, 0, 0));
        var slot2 = new TimeSlot(
            new DateTime(2026, 2, 13, 14, 0, 0),
            new DateTime(2026, 2, 13, 16, 0, 0));

        // Act
        var timeBetween = slot1.TimeBetween(slot2);

        // Assert
        timeBetween.Should().Be(TimeSpan.FromHours(2));
    }

    [Fact]
    public void TimeBetween_WithOverlappingSlots_ShouldReturnNull()
    {
        // Arrange
        var slot1 = new TimeSlot(
            new DateTime(2026, 2, 13, 10, 0, 0),
            new DateTime(2026, 2, 13, 12, 0, 0));
        var slot2 = new TimeSlot(
            new DateTime(2026, 2, 13, 11, 0, 0),
            new DateTime(2026, 2, 13, 13, 0, 0));

        // Act
        var timeBetween = slot1.TimeBetween(slot2);

        // Assert
        timeBetween.Should().BeNull();
    }
}
