using ScheduleGenerator.Application.Models;
using ScheduleGenerator.Domain.Entities;

namespace ScheduleGenerator.Application.Services;

/// <summary>
/// Service for generating matches based on tournament format
/// </summary>
public interface IMatchGenerationService
{
    /// <summary>
    /// Generates matches based on the tournament format and teams
    /// </summary>
    /// <param name="tournament">Tournament with teams and format configuration</param>
    /// <param name="formatConfig">Format-specific configuration</param>
    /// <returns>List of generated matches</returns>
    List<Match> GenerateMatches(Tournament tournament, FormatConfiguration formatConfig);
}
