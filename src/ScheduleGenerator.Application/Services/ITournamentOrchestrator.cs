using ScheduleGenerator.Application.Models;

namespace ScheduleGenerator.Application.Services;

/// <summary>
/// Main orchestrator service that coordinates the entire tournament scheduling pipeline
/// </summary>
public interface ITournamentOrchestrator
{
    /// <summary>
    /// Orchestrates the complete scheduling process from definition to output
    /// </summary>
    /// <param name="definition">Tournament definition from user input</param>
    /// <returns>Complete schedule output with diagnostics</returns>
    Task<ScheduleOutput> GenerateScheduleAsync(TournamentDefinition definition);
}
