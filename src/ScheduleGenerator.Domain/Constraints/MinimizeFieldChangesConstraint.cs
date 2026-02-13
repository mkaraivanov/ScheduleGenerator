using ScheduleGenerator.Domain.Entities;
using ScheduleGenerator.Domain.ValueObjects;

namespace ScheduleGenerator.Domain.Constraints;

/// <summary>
/// Soft constraint that encourages minimizing field changes for teams.
/// </summary>
public class MinimizeFieldChangesConstraint : IConstraint
{
    public string Name => "Minimize Field Changes";
    public ConstraintType Type => ConstraintType.Soft;
    public double Weight { get; }

    public MinimizeFieldChangesConstraint(double weight = 0.3)
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

        foreach (var team in tournament.Teams)
        {
            var teamMatches = scheduledMatches
                .Where(m => m.InvolvesTeam(team.Id))
                .OrderBy(m => m.AssignedTimeSlot!.Start)
                .ToList();

            if (teamMatches.Count < 2)
            {
                continue;
            }

            var fieldChanges = 0;
            for (int i = 0; i < teamMatches.Count - 1; i++)
            {
                if (teamMatches[i].AssignedFieldId != teamMatches[i + 1].AssignedFieldId)
                {
                    fieldChanges++;
                }
            }

            if (fieldChanges > teamMatches.Count / 2)
            {
                var field1 = tournament.Fields.FirstOrDefault(f => f.Id == teamMatches[0].AssignedFieldId);
                violations.Add(new ConstraintViolation(
                    Name,
                    $"Team '{team.Name}' changes fields {fieldChanges} times across {teamMatches.Count} matches",
                    ConstraintSeverity.Warning,
                    isHardConstraint: false,
                    penalty: Weight * fieldChanges,
                    affectedEntityIds: new[] { team.Id }));
            }
        }

        return violations;
    }
}
