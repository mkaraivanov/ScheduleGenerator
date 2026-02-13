using ScheduleGenerator.Domain.Entities;

namespace ScheduleGenerator.Application.Models;

/// <summary>
/// Represents a potential time slot for a match
/// </summary>
public record Slot
{
    public required Field Field { get; init; }
    public required DateTime StartTime { get; init; }
    public required DateTime EndTime { get; init; }

    public bool OverlapsWith(Slot other)
    {
        return Field.Equals(other.Field) &&
               StartTime < other.EndTime &&
               EndTime > other.StartTime;
    }
}
