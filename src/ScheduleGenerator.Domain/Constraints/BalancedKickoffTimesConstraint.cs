using ScheduleGenerator.Domain.Entities;
using ScheduleGenerator.Domain.ValueObjects;

namespace ScheduleGenerator.Domain.Constraints;

/// <summary>
/// Soft constraint that encourages balanced distribution of kickoff times across teams.
/// </summary>
public class BalancedKickoffTimesConstraint : IConstraint
{
    public string Name => "Balanced Kickoff Times";
    public ConstraintType Type => ConstraintType.Soft;
    public double Weight { get; }

    public BalancedKickoffTimesConstraint(double weight = 0.5)
    {
        Weight = weight;
    }

    public IEnumerable<ConstraintViolation> Evaluate(Schedule schedule, Tournament tournament)
    {
        var violations = new List<ConstraintViolation>();
        var scheduledMatches = schedule.ScheduledMatches.Where(m => m.IsScheduled).ToList();

        if (!scheduledMatches.Any())
        {
            return violations;
        }

        // Define early and late time thresholds
        var earlyThreshold = new TimeOnly(10, 0);
        var lateThreshold = new TimeOnly(18, 0);

        foreach (var team in tournament.Teams)
        {
            var teamMatches = scheduledMatches
                .Where(m => m.InvolvesTeam(team.Id))
                .ToList();

            if (teamMatches.Count < 2)
            {
                continue;
            }

            var earlyMatches = teamMatches.Count(m => 
                TimeOnly.FromDateTime(m.AssignedTimeSlot!.Start) < earlyThreshold);
            
            var lateMatches = teamMatches.Count(m => 
                TimeOnly.FromDateTime(m.AssignedTimeSlot!.Start) >= lateThreshold);

            // Check if team has disproportionate early or late matches
            var totalMatches = teamMatches.Count;
            var earlyRatio = (double)earlyMatches / totalMatches;
            var lateRatio = (double)lateMatches / totalMatches;

            if (earlyRatio > 0.6)
            {
                violations.Add(new ConstraintViolation(
                    Name,
                    $"Team '{team.Name}' has {earlyMatches} out of {totalMatches} early matches (>{earlyRatio:P0})",
                    ConstraintSeverity.Warning,
                    isHardConstraint: false,
                    penalty: Weight * Math.Abs(earlyRatio - 0.5),
                    affectedEntityIds: new[] { team.Id }));
            }

            if (lateRatio > 0.6)
            {
                violations.Add(new ConstraintViolation(
                    Name,
                    $"Team '{team.Name}' has {lateMatches} out of {totalMatches} late matches (>{lateRatio:P0})",
                    ConstraintSeverity.Warning,
                    isHardConstraint: false,
                    penalty: Weight * Math.Abs(lateRatio - 0.5),
                    affectedEntityIds: new[] { team.Id }));
            }
        }

        return violations;
    }
}
