namespace ScheduleGenerator.Domain.ValueObjects;

/// <summary>
/// Represents a unique scheduling slot combining a field and a time slot.
/// </summary>
public record Slot
{
    public Guid Id { get; init; }
    public Guid FieldId { get; init; }
    public TimeSlot TimeSlot { get; init; }

    public Slot(Guid id, Guid fieldId, TimeSlot timeSlot)
    {
        ArgumentNullException.ThrowIfNull(timeSlot, nameof(timeSlot));

        if (id == Guid.Empty)
        {
            throw new ArgumentException("Slot ID cannot be empty.", nameof(id));
        }

        if (fieldId == Guid.Empty)
        {
            throw new ArgumentException("Field ID cannot be empty.", nameof(fieldId));
        }

        Id = id;
        FieldId = fieldId;
        TimeSlot = timeSlot;
    }

    /// <summary>
    /// Checks if this slot overlaps in time with another slot (regardless of field).
    /// </summary>
    public bool OverlapsInTime(Slot other)
    {
        return TimeSlot.OverlapsWith(other.TimeSlot);
    }

    /// <summary>
    /// Checks if this slot is on the same field as another slot.
    /// </summary>
    public bool IsOnSameField(Slot other)
    {
        return FieldId == other.FieldId;
    }

    /// <summary>
    /// Checks if this slot conflicts with another slot (same field and overlapping time).
    /// </summary>
    public bool ConflictsWith(Slot other)
    {
        return IsOnSameField(other) && OverlapsInTime(other);
    }
}
