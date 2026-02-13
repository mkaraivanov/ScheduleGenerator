using ScheduleGenerator.Domain.Entities;

namespace ScheduleGenerator.Domain.Formats;

/// <summary>
/// Generates matches for a knockout tournament with bracket structure.
/// </summary>
public class KnockoutGenerator : TournamentFormatGenerator
{
    protected override IEnumerable<Match> GenerateMatchesCore(List<Team> teams, TournamentFormat format)
    {
        var matches = new List<Match>();
        var teamList = new List<Team>(teams);

        // Find next power of 2 to determine bracket size
        var bracketSize = GetNextPowerOfTwo(teamList.Count);
        var byesNeeded = bracketSize - teamList.Count;

        // Add BYE teams if needed
        for (int i = 0; i < byesNeeded; i++)
        {
            teamList.Add(CreateByeTeam());
        }

        // Generate matches for each round
        var currentRoundTeams = teamList;
        var roundNumber = 1;
        var previousRoundMatches = new List<Match>();

        while (currentRoundTeams.Count > 1)
        {
            var stage = GetMatchStage(currentRoundTeams.Count);
            var roundMatches = GenerateRoundMatches(currentRoundTeams, stage, roundNumber, previousRoundMatches);
            
            // Filter out matches involving BYE teams
            var nonByeMatches = roundMatches.Where(m => 
                !teamList.Any(t => t.Name == "BYE" && (t.Id == m.TeamAId || t.Id == m.TeamBId))).ToList();
            
            matches.AddRange(nonByeMatches);

            // Prepare for next round (winners advance)
            currentRoundTeams = GetPlaceholderTeamsForNextRound(currentRoundTeams.Count / 2);
            previousRoundMatches = roundMatches;
            roundNumber++;
        }

        return matches;
    }

    private List<Match> GenerateRoundMatches(
        List<Team> teams, 
        MatchStage stage, 
        int roundNumber,
        List<Match> previousRoundMatches)
    {
        var matches = new List<Match>();

        for (int i = 0; i < teams.Count; i += 2)
        {
            var teamA = teams[i];
            var teamB = teams[i + 1];

            var prerequisiteMatchIds = new List<Guid>();

            // For rounds after the first, add prerequisites
            if (previousRoundMatches.Any())
            {
                // Each match depends on two matches from the previous round
                var prereq1Index = i;
                var prereq2Index = i + 1;

                if (prereq1Index < previousRoundMatches.Count)
                {
                    prerequisiteMatchIds.Add(previousRoundMatches[prereq1Index].Id);
                }

                if (prereq2Index < previousRoundMatches.Count)
                {
                    prerequisiteMatchIds.Add(previousRoundMatches[prereq2Index].Id);
                }
            }

            matches.Add(new Match(
                teamA.Id,
                teamB.Id,
                stage,
                roundNumber,
                null,
                prerequisiteMatchIds));
        }

        return matches;
    }

    private MatchStage GetMatchStage(int teamsRemaining)
    {
        return teamsRemaining switch
        {
            2 => MatchStage.Final,
            4 => MatchStage.SemiFinal,
            8 => MatchStage.QuarterFinal,
            16 => MatchStage.RoundOf16,
            32 => MatchStage.RoundOf32,
            _ => MatchStage.RoundOf32
        };
    }

    private int GetNextPowerOfTwo(int n)
    {
        if (n <= 0) return 1;
        
        int power = 1;
        while (power < n)
        {
            power *= 2;
        }
        return power;
    }

    private List<Team> GetPlaceholderTeamsForNextRound(int count)
    {
        // Create placeholder teams for the next round
        // In reality, these would be winners from the previous round
        var placeholders = new List<Team>();
        for (int i = 0; i < count; i++)
        {
            placeholders.Add(new Team($"Winner_{i + 1}"));
        }
        return placeholders;
    }

    protected override void ValidateTeams(List<Team> teams, TournamentFormat format)
    {
        base.ValidateTeams(teams, format);

        if (teams.Count < 2)
        {
            throw new ArgumentException("Knockout tournament requires at least 2 teams.", nameof(teams));
        }
    }
}
