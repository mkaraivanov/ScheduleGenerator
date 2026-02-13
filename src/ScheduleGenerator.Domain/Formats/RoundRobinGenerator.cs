using ScheduleGenerator.Domain.Entities;

namespace ScheduleGenerator.Domain.Formats;

/// <summary>
/// Generates matches for a round-robin tournament using the circle method.
/// </summary>
public class RoundRobinGenerator : TournamentFormatGenerator
{
    protected override IEnumerable<Match> GenerateMatchesCore(List<Team> teams, TournamentFormat format)
    {
        var matches = new List<Match>();
        var teamList = new List<Team>(teams);

        // Add BYE team if odd number of teams
        if (teamList.Count % 2 != 0)
        {
            teamList.Add(CreateByeTeam());
        }

        var n = teamList.Count;
        var rounds = n - 1;
        var matchesPerRound = n / 2;

        for (int leg = 1; leg <= format.Legs; leg++)
        {
            for (int round = 1; round <= rounds; round++)
            {
                var roundMatches = GenerateRoundMatches(teamList, round, leg);
                matches.AddRange(roundMatches);
            }
        }

        // Filter out matches involving BYE team
        return matches.Where(m => 
            !teams.Any(t => t.Name == "BYE" && (t.Id == m.TeamAId || t.Id == m.TeamBId)));
    }

    private List<Match> GenerateRoundMatches(List<Team> teams, int round, int leg)
    {
        var matches = new List<Match>();
        var n = teams.Count;

        // Use circle method: fix one team and rotate others
        for (int i = 0; i < n / 2; i++)
        {
            int home = i;
            int away = n - 1 - i;

            if (round > 1)
            {
                // Rotate all teams except the first one
                if (home != 0)
                {
                    home = ((home + round - 1) % (n - 1));
                    if (home == 0) home = n - 1;
                }

                if (away != 0)
                {
                    away = ((away + round - 1) % (n - 1));
                    if (away == 0) away = n - 1;
                }
            }

            var teamA = teams[home];
            var teamB = teams[away];

            // Skip matches involving BYE
            if (teamA.Name == "BYE" || teamB.Name == "BYE")
            {
                continue;
            }

            // Calculate actual round number considering legs
            var actualRound = (leg - 1) * (n - 1) + round;

            // For second leg, swap home and away
            if (leg > 1 && leg % 2 == 0)
            {
                (teamA, teamB) = (teamB, teamA);
            }

            matches.Add(new Match(
                teamA.Id,
                teamB.Id,
                MatchStage.RoundRobin,
                actualRound));
        }

        return matches;
    }

    protected override void ValidateTeams(List<Team> teams, TournamentFormat format)
    {
        base.ValidateTeams(teams, format);

        if (teams.Count < 2)
        {
            throw new ArgumentException("Round-robin requires at least 2 teams.", nameof(teams));
        }
    }
}
