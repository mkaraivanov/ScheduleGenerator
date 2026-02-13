using ScheduleGenerator.Application.Models;
using ScheduleGenerator.Domain.Entities;

namespace ScheduleGenerator.Application.Services;

/// <summary>
/// Service for generating all potential time slots from fields and time windows
/// </summary>
public class SlotGenerationService : ISlotGenerationService
{
    /// <summary>
    /// Generates all possible slots for matches based on field availability and scheduling rules
    /// </summary>
    public List<Slot> GenerateSlots(List<Field> fields, SchedulingRules rules)
    {
        var slots = new List<Slot>();

        foreach (var field in fields)
        {
            foreach (var window in field.AvailabilityWindows)
            {
                var currentStart = window.Start;
                var slotDuration = TimeSpan.FromMinutes(rules.MatchDurationMinutes);
                var bufferDuration = TimeSpan.FromMinutes(rules.BufferBetweenMatchesMinutes);
                var totalSlotTime = slotDuration + bufferDuration;

                while (currentStart + slotDuration <= window.End)
                {
                    var slot = new Slot
                    {
                        Field = field,
                        StartTime = currentStart,
                        EndTime = currentStart + slotDuration
                    };

                    slots.Add(slot);

                    // Move to next slot (current end + buffer)
                    currentStart = slot.EndTime + bufferDuration;
                }
            }
        }

        return slots;
    }
}
