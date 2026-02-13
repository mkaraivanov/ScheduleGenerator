using ScheduleGenerator.Domain.Entities;
using ScheduleGenerator.Domain.ValueObjects;

namespace ScheduleGenerator.Domain.Constraints;

/// <summary>
/// Ensures that only one match is scheduled per field per time slot.
/// </summary>
public class OneMatchPerSlotConstraint : IConstraint
{
    public string Name => "One Match Per Slot";
    public ConstraintType Type => ConstraintType.Hard;
    public double Weight => 1.0;

    public IEnumerable<ConstraintViolation> Evaluate(Schedule schedule, Tournament tournament)
    {
        var violations = new List<ConstraintViolation>();
        var scheduledMatches = schedule.ScheduledMatches.Where(m => m.IsScheduled).ToList();

        // Group matches by field
        var matchesByField = scheduledMatches
            .GroupBy(m => m.AssignedFieldId!.Value);

        foreach (var fieldGroup in matchesByField)
        {
            var fieldId = fieldGroup.Key;
            var field = tournament.Fields.FirstOrDefault(f => f.Id == fieldId);
            var fieldName = field?.Name ?? fieldId.ToString();

            var matches = fieldGroup.OrderBy(m => m.AssignedTimeSlot!.Start).ToList();

            for (int i = 0; i < matches.Count - 1; i++)
            {
                var currentMatch = matches[i];
                var nextMatch = matches[i + 1];

                if (currentMatch.AssignedTimeSlot!.OverlapsWith(nextMatch.AssignedTimeSlot!))
                {
                    violations.Add(new ConstraintViolation(
                        Name,
                        $"Field '{fieldName}' has overlapping matches at {currentMatch.AssignedTimeSlot.Start} and {nextMatch.AssignedTimeSlot.Start}",
                        ConstraintSeverity.Critical,
                        isHardConstraint: true,
                        penalty: 0,
                        affectedEntityIds: new[] { fieldId, currentMatch.Id, nextMatch.Id }));
                }
            }
        }

        return violations;
    }
}
