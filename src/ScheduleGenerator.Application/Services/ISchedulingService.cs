using ScheduleGenerator.Application.Models;
using ScheduleGenerator.Domain.Entities;

namespace ScheduleGenerator.Application.Services;

/// <summary>
/// Service for scheduling matches into slots with constraint satisfaction
/// </summary>
public interface ISchedulingService
{
    /// <summary>
    /// Schedules matches into available slots while satisfying constraints
    /// </summary>
    /// <param name="matches">Matches to schedule</param>
    /// <param name="slots">Available time slots</param>
    /// <param name="tournament">Tournament with teams and configuration</param>
    /// <param name="rules">Scheduling rules</param>
    /// <param name="constraints">Constraint configuration</param>
    /// <returns>Complete schedule with diagnostics</returns>
    Schedule? ScheduleMatches(
        List<Match> matches,
        List<Slot> slots,
        Tournament tournament,
        SchedulingRules rules,
        ConstraintConfiguration? constraints);
}
