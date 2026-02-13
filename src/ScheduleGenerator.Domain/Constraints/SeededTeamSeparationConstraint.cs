using ScheduleGenerator.Domain.Entities;
using ScheduleGenerator.Domain.ValueObjects;

namespace ScheduleGenerator.Domain.Constraints;

/// <summary>
/// Soft constraint that encourages keeping top-seeded teams apart in early rounds.
/// </summary>
public class SeededTeamSeparationConstraint : IConstraint
{
    private readonly int _topSeedsToSeparate;
    private readonly int _earlyRoundThreshold;

    public string Name => "Seeded Team Separation";
    public ConstraintType Type => ConstraintType.Soft;
    public double Weight { get; }

    public SeededTeamSeparationConstraint(int topSeedsToSeparate = 4, int earlyRoundThreshold = 3, double weight = 0.6)
    {
        _topSeedsToSeparate = topSeedsToSeparate;
        _earlyRoundThreshold = earlyRoundThreshold;
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

        // Get top seeded teams
        var topSeededTeams = tournament.Teams
            .Where(t => t.Seed.HasValue)
            .OrderBy(t => t.Seed!.Value)
            .Take(_topSeedsToSeparate)
            .ToList();

        if (topSeededTeams.Count < 2)
        {
            return violations;
        }

        var topSeededTeamIds = topSeededTeams.Select(t => t.Id).ToHashSet();

        // Check early round matches
        var earlyMatches = scheduledMatches
            .Where(m => m.RoundNumber <= _earlyRoundThreshold)
            .ToList();

        foreach (var match in earlyMatches)
        {
            var teamAIsTopSeed = topSeededTeamIds.Contains(match.TeamAId);
            var teamBIsTopSeed = topSeededTeamIds.Contains(match.TeamBId);

            if (teamAIsTopSeed && teamBIsTopSeed)
            {
                var teamA = topSeededTeams.First(t => t.Id == match.TeamAId);
                var teamB = topSeededTeams.First(t => t.Id == match.TeamBId);

                violations.Add(new ConstraintViolation(
                    Name,
                    $"Top seeded teams '{teamA.Name}' (seed {teamA.Seed}) and '{teamB.Name}' (seed {teamB.Seed}) " +
                    $"meet in early round {match.RoundNumber}",
                    ConstraintSeverity.Warning,
                    isHardConstraint: false,
                    penalty: Weight * (10 - match.RoundNumber),
                    affectedEntityIds: new[] { match.Id, teamA.Id, teamB.Id }));
            }
        }

        return violations;
    }
}
