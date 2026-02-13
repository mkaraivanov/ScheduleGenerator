using ScheduleGenerator.Domain.ValueObjects;

namespace ScheduleGenerator.Domain.Entities;

/// <summary>
/// Represents a field where matches can be played.
/// </summary>
public class Field
{
    private readonly List<TimeSlot> _availabilityWindows = new();

    public Guid Id { get; init; }
    public string Name { get; private set; }
    public IReadOnlyCollection<TimeSlot> AvailabilityWindows => _availabilityWindows.AsReadOnly();

    public Field(string name, IEnumerable<TimeSlot>? availabilityWindows = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Field name cannot be empty.", nameof(name));
        }

        Id = Guid.NewGuid();
        Name = name;
        
        if (availabilityWindows != null)
        {
            _availabilityWindows.AddRange(availabilityWindows);
        }
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Field name cannot be empty.", nameof(name));
        }
        Name = name;
    }

    public void AddAvailabilityWindow(TimeSlot timeSlot)
    {
        ArgumentNullException.ThrowIfNull(timeSlot, nameof(timeSlot));
        _availabilityWindows.Add(timeSlot);
    }

    public void RemoveAvailabilityWindow(TimeSlot timeSlot)
    {
        _availabilityWindows.Remove(timeSlot);
    }

    public void ClearAvailabilityWindows()
    {
        _availabilityWindows.Clear();
    }

    /// <summary>
    /// Checks if the field is available during a specific time slot.
    /// </summary>
    public bool IsAvailableDuring(TimeSlot timeSlot)
    {
        if (!_availabilityWindows.Any())
        {
            return true; // If no windows specified, field is always available
        }

        return _availabilityWindows.Any(window => 
            window.Contains(timeSlot.Start) && window.Contains(timeSlot.End.AddSeconds(-1)));
    }

    public override string ToString() => Name;
}
