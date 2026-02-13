using ScheduleGenerator.Domain.Entities;
using ScheduleGenerator.Domain.ValueObjects;

namespace ScheduleGenerator.Domain.Constraints;

/// <summary>
/// Interface for constraints that evaluate schedule validity and quality.
/// </summary>
public interface IConstraint
{
    /// <summary>
    /// Gets the name of the constraint.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the type of constraint (Hard or Soft).
    /// </summary>
    ConstraintType Type { get; }

    /// <summary>
    /// Gets the weight of the constraint for optimization (used for soft constraints).
    /// </summary>
    double Weight { get; }

    /// <summary>
    /// Evaluates the constraint against a schedule.
    /// </summary>
    /// <param name="schedule">The schedule to evaluate.</param>
    /// <param name="tournament">The tournament context.</param>
    /// <returns>A collection of constraint violations (empty if satisfied).</returns>
    IEnumerable<ConstraintViolation> Evaluate(Schedule schedule, Tournament tournament);
}

/// <summary>
/// Type of constraint.
/// </summary>
public enum ConstraintType
{
    /// <summary>
    /// Hard constraint - must be satisfied for a valid schedule.
    /// </summary>
    Hard,

    /// <summary>
    /// Soft constraint - should be optimized but not required.
    /// </summary>
    Soft
}
