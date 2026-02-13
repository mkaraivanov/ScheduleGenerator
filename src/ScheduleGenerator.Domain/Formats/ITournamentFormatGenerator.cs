using ScheduleGenerator.Domain.Entities;

namespace ScheduleGenerator.Domain.Formats;

/// <summary>
/// Interface for generating matches based on tournament format.
/// </summary>
public interface ITournamentFormatGenerator
{
    /// <summary>
    /// Generates matches for the given teams and tournament format.
    /// </summary>
    /// <param name="teams">The teams participating in the tournament.</param>
    /// <param name="format">The tournament format.</param>
    /// <returns>A collection of matches to be scheduled.</returns>
    IEnumerable<Match> GenerateMatches(IEnumerable<Team> teams, TournamentFormat format);
}
