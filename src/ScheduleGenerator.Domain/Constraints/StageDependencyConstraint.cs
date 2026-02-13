using ScheduleGenerator.Domain.Entities;
using ScheduleGenerator.Domain.ValueObjects;

namespace ScheduleGenerator.Domain.Constraints;

/// <summary>
/// Ensures that matches respect stage dependencies (e.g., knockout matches are scheduled after their prerequisites).
/// </summary>
public class StageDependencyConstraint : IConstraint
{
    public string Name => "Stage Dependency";
    public ConstraintType Type => ConstraintType.Hard;
    public double Weight => 1.0;

    public IEnumerable<ConstraintViolation> Evaluate(Schedule schedule, Tournament tournament)
    {
        var violations = new List<ConstraintViolation>();
        var scheduledMatches = schedule.ScheduledMatches.Where(m => m.IsScheduled).ToList();
        var matchLookup = scheduledMatches.ToDictionary(m => m.Id);

        foreach (var match in scheduledMatches)
        {
            if (!match.PrerequisiteMatchIds.Any())
            {
                continue;
            }

            foreach (var prerequisiteId in match.PrerequisiteMatchIds)
            {
                if (!matchLookup.TryGetValue(prerequisiteId, out var prerequisiteMatch))
                {
                    violations.Add(new ConstraintViolation(
                        Name,
                        $"Match {match.Id} has prerequisite {prerequisiteId} that is not in the schedule",
                        ConstraintSeverity.Critical,
                        isHardConstraint: true,
                        penalty: 0,
                        affectedEntityIds: new[] { match.Id, prerequisiteId }));
                    continue;
                }

                if (!prerequisiteMatch.IsScheduled)
                {
                    violations.Add(new ConstraintViolation(
                        Name,
                        $"Match {match.Id} depends on unscheduled match {prerequisiteId}",
                        ConstraintSeverity.Critical,
                        isHardConstraint: true,
                        penalty: 0,
                        affectedEntityIds: new[] { match.Id, prerequisiteId }));
                    continue;
                }

                // Check if prerequisite match ends before dependent match starts
                if (prerequisiteMatch.AssignedTimeSlot!.End > match.AssignedTimeSlot!.Start)
                {
                    violations.Add(new ConstraintViolation(
                        Name,
                        $"Match at {match.AssignedTimeSlot.Start:g} depends on match at {prerequisiteMatch.AssignedTimeSlot.Start:g} " +
                        $"which ends at {prerequisiteMatch.AssignedTimeSlot.End:g}",
                        ConstraintSeverity.Critical,
                        isHardConstraint: true,
                        penalty: 0,
                        affectedEntityIds: new[] { match.Id, prerequisiteId }));
                }
            }
        }

        return violations;
    }
}
