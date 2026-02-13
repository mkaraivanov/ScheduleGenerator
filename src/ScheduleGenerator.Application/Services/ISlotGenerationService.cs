using ScheduleGenerator.Application.Models;
using ScheduleGenerator.Domain.Entities;

namespace ScheduleGenerator.Application.Services;

/// <summary>
/// Service for generating all potential time slots from fields and time windows
/// </summary>
public interface ISlotGenerationService
{
    /// <summary>
    /// Generates all possible slots for matches based on field availability and scheduling rules
    /// </summary>
    /// <param name="fields">Available fields with their time windows</param>
    /// <param name="rules">Scheduling rules including match duration and buffer times</param>
    /// <returns>List of all potential slots</returns>
    List<Slot> GenerateSlots(List<Field> fields, SchedulingRules rules);
}
