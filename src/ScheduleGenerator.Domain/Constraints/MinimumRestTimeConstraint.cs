using ScheduleGenerator.Domain.Entities;
using ScheduleGenerator.Domain.ValueObjects;

namespace ScheduleGenerator.Domain.Constraints;

/// <summary>
/// Ensures that teams have a minimum rest time between matches.
/// </summary>
public class MinimumRestTimeConstraint : IConstraint
{
    private readonly TimeSpan _minimumRestTime;

    public string Name => "Minimum Rest Time";
    public ConstraintType Type => ConstraintType.Hard;
    public double Weight => 1.0;

    public MinimumRestTimeConstraint(TimeSpan minimumRestTime)
    {
        if (minimumRestTime < TimeSpan.Zero)
        {
            throw new ArgumentException("Minimum rest time cannot be negative.", nameof(minimumRestTime));
        }

        _minimumRestTime = minimumRestTime;
    }

    public IEnumerable<ConstraintViolation> Evaluate(Schedule schedule, Tournament tournament)
    {
        var violations = new List<ConstraintViolation>();
        var scheduledMatches = schedule.ScheduledMatches.Where(m => m.IsScheduled).ToList();

        foreach (var team in tournament.Teams)
        {
            var teamMatches = scheduledMatches
                .Where(m => m.InvolvesTeam(team.Id))
                .OrderBy(m => m.AssignedTimeSlot!.Start)
                .ToList();

            for (int i = 0; i < teamMatches.Count - 1; i++)
            {
                var currentMatch = teamMatches[i];
                var nextMatch = teamMatches[i + 1];

                var timeBetween = currentMatch.AssignedTimeSlot!.TimeBetween(nextMatch.AssignedTimeSlot!);

                if (timeBetween.HasValue && timeBetween.Value < _minimumRestTime)
                {
                    violations.Add(new ConstraintViolation(
                        Name,
                        $"Team '{team.Name}' has insufficient rest time ({timeBetween.Value.TotalHours:F1}h) between matches. " +
                        $"Minimum required: {_minimumRestTime.TotalHours:F1}h. " +
                        $"Matches at {currentMatch.AssignedTimeSlot.Start:g} and {nextMatch.AssignedTimeSlot.Start:g}",
                        ConstraintSeverity.Critical,
                        isHardConstraint: true,
                        penalty: 0,
                        affectedEntityIds: new[] { team.Id, currentMatch.Id, nextMatch.Id }));
                }
            }
        }

        return violations;
    }
}
