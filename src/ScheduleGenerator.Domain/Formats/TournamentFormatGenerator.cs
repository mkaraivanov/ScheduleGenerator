using ScheduleGenerator.Domain.Entities;

namespace ScheduleGenerator.Domain.Formats;

/// <summary>
/// Abstract base class for tournament format generators (Template Method pattern).
/// </summary>
public abstract class TournamentFormatGenerator : ITournamentFormatGenerator
{
    public IEnumerable<Match> GenerateMatches(IEnumerable<Team> teams, TournamentFormat format)
    {
        var teamList = teams.ToList();
        
        ValidateTeams(teamList, format);
        var matches = GenerateMatchesCore(teamList, format);
        
        return matches;
    }

    /// <summary>
    /// Core match generation logic implemented by derived classes.
    /// </summary>
    protected abstract IEnumerable<Match> GenerateMatchesCore(List<Team> teams, TournamentFormat format);

    /// <summary>
    /// Validates that the teams are suitable for this format.
    /// </summary>
    protected virtual void ValidateTeams(List<Team> teams, TournamentFormat format)
    {
        if (!teams.Any())
        {
            throw new ArgumentException("At least one team is required.", nameof(teams));
        }

        if (teams.Select(t => t.Id).Distinct().Count() != teams.Count)
        {
            throw new ArgumentException("Teams must have unique IDs.", nameof(teams));
        }
    }

    /// <summary>
    /// Creates a BYE team for handling odd number of teams.
    /// </summary>
    protected Team CreateByeTeam()
    {
        return new Team("BYE");
    }
}
