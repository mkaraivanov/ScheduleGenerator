using ScheduleGenerator.Domain.Entities;
using ScheduleGenerator.Domain.ValueObjects;

namespace ScheduleGenerator.Domain.Constraints;

/// <summary>
/// Soft constraint that encourages spacing out matches against the same opponent.
/// </summary>
public class OpponentSpacingConstraint : IConstraint
{
    private readonly TimeSpan _minimumSpacing;

    public string Name => "Opponent Spacing";
    public ConstraintType Type => ConstraintType.Soft;
    public double Weight { get; }

    public OpponentSpacingConstraint(TimeSpan? minimumSpacing = null, double weight = 0.4)
    {
        _minimumSpacing = minimumSpacing ?? TimeSpan.FromDays(3);
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

        // Group matches by team pairs
        var matchesByOpponents = new Dictionary<(Guid, Guid), List<Match>>();

        foreach (var match in scheduledMatches)
        {
            var teamPair = match.TeamAId < match.TeamBId 
                ? (match.TeamAId, match.TeamBId)
                : (match.TeamBId, match.TeamAId);

            if (!matchesByOpponents.ContainsKey(teamPair))
            {
                matchesByOpponents[teamPair] = new List<Match>();
            }
            matchesByOpponents[teamPair].Add(match);
        }

        foreach (var kvp in matchesByOpponents)
        {
            var matches = kvp.Value.OrderBy(m => m.AssignedTimeSlot!.Start).ToList();
            
            if (matches.Count < 2)
            {
                continue;
            }

            for (int i = 0; i < matches.Count - 1; i++)
            {
                var currentMatch = matches[i];
                var nextMatch = matches[i + 1];

                var timeBetween = currentMatch.AssignedTimeSlot!.TimeBetween(nextMatch.AssignedTimeSlot!);

                if (timeBetween.HasValue && timeBetween.Value < _minimumSpacing)
                {
                    var team1 = tournament.Teams.FirstOrDefault(t => t.Id == kvp.Key.Item1);
                    var team2 = tournament.Teams.FirstOrDefault(t => t.Id == kvp.Key.Item2);

                    violations.Add(new ConstraintViolation(
                        Name,
                        $"Matches between '{team1?.Name}' and '{team2?.Name}' are scheduled only " +
                        $"{timeBetween.Value.TotalDays:F1} days apart (recommended: {_minimumSpacing.TotalDays:F0} days)",
                        ConstraintSeverity.Info,
                        isHardConstraint: false,
                        penalty: Weight * (_minimumSpacing.TotalDays - timeBetween.Value.TotalDays),
                        affectedEntityIds: new[] { kvp.Key.Item1, kvp.Key.Item2, currentMatch.Id, nextMatch.Id }));
                }
            }
        }

        return violations;
    }
}
