using ScheduleGenerator.Domain.Entities;
using ScheduleGenerator.Domain.ValueObjects;

namespace ScheduleGenerator.Domain.Constraints;

/// <summary>
/// Ensures that a team cannot play multiple matches simultaneously.
/// </summary>
public class NoSimultaneousMatchesConstraint : IConstraint
{
    public string Name => "No Simultaneous Matches";
    public ConstraintType Type => ConstraintType.Hard;
    public double Weight => 1.0;

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

                if (currentMatch.AssignedTimeSlot!.OverlapsWith(nextMatch.AssignedTimeSlot!))
                {
                    violations.Add(new ConstraintViolation(
                        Name,
                        $"Team '{team.Name}' has overlapping matches at {currentMatch.AssignedTimeSlot.Start} and {nextMatch.AssignedTimeSlot.Start}",
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
