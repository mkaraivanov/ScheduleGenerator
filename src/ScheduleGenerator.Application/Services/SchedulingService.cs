using ScheduleGenerator.Application.Interfaces;
using ScheduleGenerator.Application.Models;
using ScheduleGenerator.Domain.Constraints;
using ScheduleGenerator.Domain.Entities;

namespace ScheduleGenerator.Application.Services;

/// <summary>
/// Service for scheduling matches into slots with constraint satisfaction
/// </summary>
public class SchedulingService : ISchedulingService
{
    private readonly ISchedulingEngine _schedulingEngine;

    public SchedulingService(ISchedulingEngine schedulingEngine)
    {
        _schedulingEngine = schedulingEngine ?? throw new ArgumentNullException(nameof(schedulingEngine));
    }

    /// <summary>
    /// Schedules matches into available slots while satisfying constraints
    /// </summary>
    public Schedule? ScheduleMatches(
        List<Match> matches,
        List<Slot> slots,
        Tournament tournament,
        SchedulingRules rules,
        ConstraintConfiguration? constraints)
    {
        ArgumentNullException.ThrowIfNull(matches);
        ArgumentNullException.ThrowIfNull(slots);
        ArgumentNullException.ThrowIfNull(tournament);
        ArgumentNullException.ThrowIfNull(rules);

        // Build constraint list from configuration
        var constraintList = BuildConstraints(tournament, rules, constraints);

        // Use the scheduling engine to schedule matches with tournament context
        var schedule = _schedulingEngine.ScheduleAsync(
            matches,
            slots,
            constraintList,
            tournament,
            CancellationToken.None).GetAwaiter().GetResult();

        return schedule;
    }

    private List<IConstraint> BuildConstraints(
        Tournament tournament,
        SchedulingRules rules,
        ConstraintConfiguration? config)
    {
        var constraintsList = new List<IConstraint>();

        // Add hard constraints
        constraintsList.Add(new NoSimultaneousMatchesConstraint());

        if (rules.MinimumRestTimeMinutes > 0)
        {
            constraintsList.Add(new MinimumRestTimeConstraint(
                TimeSpan.FromMinutes(rules.MinimumRestTimeMinutes)));
        }

        // Add soft constraints based on weights (if configuration provided)
        if (config != null)
        {
            if (config.BalancedKickoffTimesWeight > 0)
            {
                constraintsList.Add(new BalancedKickoffTimesConstraint(
                    config.BalancedKickoffTimesWeight));
            }

            if (config.MinimizeFieldChangesWeight > 0)
            {
                constraintsList.Add(new MinimizeFieldChangesConstraint(
                    config.MinimizeFieldChangesWeight));
            }
        }

        return constraintsList;
    }
}
