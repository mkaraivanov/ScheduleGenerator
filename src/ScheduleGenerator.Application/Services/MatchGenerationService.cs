using ScheduleGenerator.Application.Models;
using ScheduleGenerator.Domain.Entities;
using ScheduleGenerator.Domain.Formats;

namespace ScheduleGenerator.Application.Services;

/// <summary>
/// Service for generating matches based on tournament format
/// </summary>
public class MatchGenerationService : IMatchGenerationService
{
    /// <summary>
    /// Generates matches based on the tournament format and teams
    /// </summary>
    public List<Match> GenerateMatches(Tournament tournament, FormatConfiguration formatConfig)
    {
        ITournamentFormatGenerator generator = formatConfig.Type.ToLowerInvariant() switch
        {
            "roundrobin" => new RoundRobinGenerator(),
            "groups" or "groupstage" => new GroupStageGenerator(),
            "knockout" => new KnockoutGenerator(),
            _ => throw new ArgumentException($"Unknown tournament format: {formatConfig.Type}", nameof(formatConfig))
        };

        var matches = generator.GenerateMatches(tournament.Teams, tournament.Format);
        return matches.ToList();
    }
}
