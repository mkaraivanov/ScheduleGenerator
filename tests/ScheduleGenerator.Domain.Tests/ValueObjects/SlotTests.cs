using FluentAssertions;
using ScheduleGenerator.Domain.ValueObjects;

namespace ScheduleGenerator.Domain.Tests.ValueObjects;

public class SlotTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateSlot()
    {
        // Arrange
        var id = Guid.NewGuid();
        var fieldId = Guid.NewGuid();
        var timeSlot = new TimeSlot(
            new DateTime(2026, 2, 13, 10, 0, 0),
            new DateTime(2026, 2, 13, 12, 0, 0));

        // Act
        var slot = new Slot(id, fieldId, timeSlot);

        // Assert
        slot.Id.Should().Be(id);
        slot.FieldId.Should().Be(fieldId);
        slot.TimeSlot.Should().Be(timeSlot);
    }

    [Fact]
    public void Constructor_WithEmptyId_ShouldThrowArgumentException()
    {
        // Arrange
        var fieldId = Guid.NewGuid();
        var timeSlot = new TimeSlot(
            new DateTime(2026, 2, 13, 10, 0, 0),
            new DateTime(2026, 2, 13, 12, 0, 0));

        // Act
        var act = () => new Slot(Guid.Empty, fieldId, timeSlot);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Slot ID cannot be empty*");
    }

    [Fact]
    public void Constructor_WithEmptyFieldId_ShouldThrowArgumentException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var timeSlot = new TimeSlot(
            new DateTime(2026, 2, 13, 10, 0, 0),
            new DateTime(2026, 2, 13, 12, 0, 0));

        // Act
        var act = () => new Slot(id, Guid.Empty, timeSlot);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Field ID cannot be empty*");
    }

    [Fact]
    public void Constructor_WithNullTimeSlot_ShouldThrowArgumentNullException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var fieldId = Guid.NewGuid();

        // Act
        var act = () => new Slot(id, fieldId, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Equality_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var id = Guid.NewGuid();
        var fieldId = Guid.NewGuid();
        var timeSlot = new TimeSlot(
            new DateTime(2026, 2, 13, 10, 0, 0),
            new DateTime(2026, 2, 13, 12, 0, 0));

        var slot1 = new Slot(id, fieldId, timeSlot);
        var slot2 = new Slot(id, fieldId, timeSlot);

        // Act & Assert
        slot1.Should().Be(slot2);
    }

    [Fact]
    public void Equality_WithDifferentIds_ShouldNotBeEqual()
    {
        // Arrange
        var fieldId = Guid.NewGuid();
        var timeSlot = new TimeSlot(
            new DateTime(2026, 2, 13, 10, 0, 0),
            new DateTime(2026, 2, 13, 12, 0, 0));

        var slot1 = new Slot(Guid.NewGuid(), fieldId, timeSlot);
        var slot2 = new Slot(Guid.NewGuid(), fieldId, timeSlot);

        // Act & Assert
        slot1.Should().NotBe(slot2);
    }

    [Fact]
    public void OverlapsInTime_WithOverlappingSlots_ShouldReturnTrue()
    {
        // Arrange
        var fieldId = Guid.NewGuid();
        var slot1 = new Slot(
            Guid.NewGuid(),
            fieldId,
            new TimeSlot(
                new DateTime(2026, 2, 13, 10, 0, 0),
                new DateTime(2026, 2, 13, 12, 0, 0)));
        var slot2 = new Slot(
            Guid.NewGuid(),
            fieldId,
            new TimeSlot(
                new DateTime(2026, 2, 13, 11, 0, 0),
                new DateTime(2026, 2, 13, 13, 0, 0)));

        // Act
        var overlaps = slot1.OverlapsInTime(slot2);

        // Assert
        overlaps.Should().BeTrue();
    }

    [Fact]
    public void IsOnSameField_WithSameFieldId_ShouldReturnTrue()
    {
        // Arrange
        var fieldId = Guid.NewGuid();
        var timeSlot1 = new TimeSlot(
            new DateTime(2026, 2, 13, 10, 0, 0),
            new DateTime(2026, 2, 13, 12, 0, 0));
        var timeSlot2 = new TimeSlot(
            new DateTime(2026, 2, 13, 14, 0, 0),
            new DateTime(2026, 2, 13, 16, 0, 0));

        var slot1 = new Slot(Guid.NewGuid(), fieldId, timeSlot1);
        var slot2 = new Slot(Guid.NewGuid(), fieldId, timeSlot2);

        // Act
        var sameField = slot1.IsOnSameField(slot2);

        // Assert
        sameField.Should().BeTrue();
    }

    [Fact]
    public void ConflictsWith_WithSameFieldAndOverlappingTime_ShouldReturnTrue()
    {
        // Arrange
        var fieldId = Guid.NewGuid();
        var slot1 = new Slot(
            Guid.NewGuid(),
            fieldId,
            new TimeSlot(
                new DateTime(2026, 2, 13, 10, 0, 0),
                new DateTime(2026, 2, 13, 12, 0, 0)));
        var slot2 = new Slot(
            Guid.NewGuid(),
            fieldId,
            new TimeSlot(
                new DateTime(2026, 2, 13, 11, 0, 0),
                new DateTime(2026, 2, 13, 13, 0, 0)));

        // Act
        var conflicts = slot1.ConflictsWith(slot2);

        // Assert
        conflicts.Should().BeTrue();
    }

    [Fact]
    public void ConflictsWith_WithDifferentFields_ShouldReturnFalse()
    {
        // Arrange
        var slot1 = new Slot(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new TimeSlot(
                new DateTime(2026, 2, 13, 10, 0, 0),
                new DateTime(2026, 2, 13, 12, 0, 0)));
        var slot2 = new Slot(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new TimeSlot(
                new DateTime(2026, 2, 13, 11, 0, 0),
                new DateTime(2026, 2, 13, 13, 0, 0)));

        // Act
        var conflicts = slot1.ConflictsWith(slot2);

        // Assert
        conflicts.Should().BeFalse();
    }
}
