using ScheduleGenerator.Domain.Entities;

namespace ScheduleGenerator.Domain.Formats;

/// <summary>
/// Generates matches for a group stage tournament with groups playing round-robin.
/// </summary>
public class GroupStageGenerator : TournamentFormatGenerator
{
    private readonly RoundRobinGenerator _roundRobinGenerator;

    public GroupStageGenerator()
    {
        _roundRobinGenerator = new RoundRobinGenerator();
    }

    protected override IEnumerable<Match> GenerateMatchesCore(List<Team> teams, TournamentFormat format)
    {
        if (!format.GroupCount.HasValue || format.GroupCount.Value < 2)
        {
            throw new ArgumentException("Group stage requires at least 2 groups.", nameof(format));
        }

        var matches = new List<Match>();
        var groupCount = format.GroupCount.Value;
        var groups = DistributeTeamsIntoGroups(teams, groupCount);

        for (int groupIndex = 0; groupIndex < groups.Count; groupIndex++)
        {
            var group = groups[groupIndex];
            var groupIdentifier = GetGroupName(groupIndex);

            // Generate round-robin matches for this group
            var groupFormat = TournamentFormat.RoundRobin(1);
            var groupMatches = _roundRobinGenerator.GenerateMatches(group, groupFormat);

            // Update group identifier for each match
            foreach (var match in groupMatches)
            {
                var groupMatch = new Match(
                    match.TeamAId,
                    match.TeamBId,
                    MatchStage.GroupStage,
                    match.RoundNumber,
                    groupIdentifier);

                matches.Add(groupMatch);
            }
        }

        return matches;
    }

    /// <summary>
    /// Distributes teams into groups, attempting to balance group sizes and respect seeding.
    /// </summary>
    private List<List<Team>> DistributeTeamsIntoGroups(List<Team> teams, int groupCount)
    {
        var groups = new List<List<Team>>();
        for (int i = 0; i < groupCount; i++)
        {
            groups.Add(new List<Team>());
        }

        // Sort teams by seed (if available), then by name
        var sortedTeams = teams
            .OrderBy(t => t.Seed ?? int.MaxValue)
            .ThenBy(t => t.Name)
            .ToList();

        // Distribute teams in snake pattern to balance groups
        // e.g., for 4 groups: Group 0, 1, 2, 3, 3, 2, 1, 0, 0, 1, ...
        int currentGroup = 0;
        int direction = 1;

        foreach (var team in sortedTeams)
        {
            groups[currentGroup].Add(team);

            currentGroup += direction;

            if (currentGroup >= groupCount)
            {
                currentGroup = groupCount - 1;
                direction = -1;
            }
            else if (currentGroup < 0)
            {
                currentGroup = 0;
                direction = 1;
            }
        }

        return groups;
    }

    private string GetGroupName(int groupIndex)
    {
        // Generate group names: A, B, C, etc.
        return ((char)('A' + groupIndex)).ToString();
    }

    protected override void ValidateTeams(List<Team> teams, TournamentFormat format)
    {
        base.ValidateTeams(teams, format);

        if (!format.GroupCount.HasValue)
        {
            throw new ArgumentException("Group count must be specified for group stage.", nameof(format));
        }

        if (teams.Count < format.GroupCount.Value * 2)
        {
            throw new ArgumentException(
                $"Not enough teams for {format.GroupCount.Value} groups. Need at least {format.GroupCount.Value * 2} teams.",
                nameof(teams));
        }
    }
}
