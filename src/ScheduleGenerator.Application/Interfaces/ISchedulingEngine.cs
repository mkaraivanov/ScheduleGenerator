using ScheduleGenerator.Application.Models;
using ScheduleGenerator.Domain.Constraints;
using ScheduleGenerator.Domain.Entities;

namespace ScheduleGenerator.Application.Interfaces;

/// <summary>
/// Defines the contract for scheduling engines that assign matches to slots.
/// </summary>
public interface ISchedulingEngine
{
    /// <summary>
    /// Schedules the given matches into available slots while respecting constraints.
    /// </summary>
    /// <param name="matches">The matches to schedule</param>
    /// <param name="slots">Available time slots</param>
    /// <param name="constraints">Constraints to satisfy or optimize</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A schedule with assigned matches or diagnostic information if infeasible</returns>
    Task<Schedule> ScheduleAsync(
        IEnumerable<Match> matches,
        IEnumerable<Slot> slots,
        IEnumerable<IConstraint> constraints,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Schedules the given matches into available slots while respecting constraints with tournament context.
    /// </summary>
    /// <param name="matches">The matches to schedule</param>
    /// <param name="slots">Available time slots</param>
    /// <param name="constraints">Constraints to satisfy or optimize</param>
    /// <param name="tournament">Tournament context for constraint evaluation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A schedule with assigned matches or diagnostic information if infeasible</returns>
    Task<Schedule> ScheduleAsync(
        IEnumerable<Match> matches,
        IEnumerable<Slot> slots,
        IEnumerable<IConstraint> constraints,
        Tournament? tournament,
        CancellationToken cancellationToken = default);
}
