using FluentValidation;
using ScheduleGenerator.Application.Models;
using ScheduleGenerator.Application.Validators;
using ScheduleGenerator.Domain.Entities;
using ScheduleGenerator.Domain.ValueObjects;

namespace ScheduleGenerator.Application.Services;

/// <summary>
/// Main orchestrator service that coordinates the entire tournament scheduling pipeline
/// </summary>
public class TournamentOrchestrator : ITournamentOrchestrator
{
    private readonly IMatchGenerationService _matchGenerationService;
    private readonly ISlotGenerationService _slotGenerationService;
    private readonly ISchedulingService _schedulingService;
    private readonly TournamentDefinitionValidator _validator;

    public TournamentOrchestrator(
        IMatchGenerationService matchGenerationService,
        ISlotGenerationService slotGenerationService,
        ISchedulingService schedulingService)
    {
        _matchGenerationService = matchGenerationService ?? throw new ArgumentNullException(nameof(matchGenerationService));
        _slotGenerationService = slotGenerationService ?? throw new ArgumentNullException(nameof(slotGenerationService));
        _schedulingService = schedulingService ?? throw new ArgumentNullException(nameof(schedulingService));
        _validator = new TournamentDefinitionValidator();
    }

    /// <summary>
    /// Orchestrates the complete scheduling process from definition to output
    /// </summary>
    public async Task<ScheduleOutput> GenerateScheduleAsync(TournamentDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition, nameof(definition));

        // Step 1: Validate input
        var validationResult = await _validator.ValidateAsync(definition);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        // Step 2: Map DTOs to Domain entities
        var tournament = MapToTournament(definition);
        var fields = MapToFields(definition.Fields);
        
        // Add fields to tournament
        foreach (var field in fields)
        {
            tournament.AddField(field);
        }

        // Step 3: Generate matches based on format
        var matches = _matchGenerationService.GenerateMatches(tournament, definition.Format);

        // Step 4: Generate available slots
        var slots = _slotGenerationService.GenerateSlots(fields, definition.Rules);

        // Step 5: Schedule matches into slots (will throw NotImplementedException in Phase 2)
        var schedule = _schedulingService.ScheduleMatches(
            matches,
            slots,
            tournament,
            definition.Rules,
            definition.Constraints);

        // Step 6: Map to output DTO
        return MapToScheduleOutput(definition.Name, schedule!);
    }

    private Tournament MapToTournament(TournamentDefinition definition)
    {
        var format = MapToTournamentFormat(definition.Format);
        var matchDuration = new MatchDuration(
            definition.Rules.MatchDurationMinutes,
            definition.Rules.BufferBetweenMatchesMinutes);
        
        var minimumRestTime = TimeSpan.FromMinutes(definition.Rules.MinimumRestTimeMinutes);
        
        var tournament = new Tournament(
            definition.Name,
            format,
            matchDuration,
            minimumRestTime);

        // Add teams
        foreach (var teamDef in definition.Teams)
        {
            var team = new Team(teamDef.Name, club: null, seed: teamDef.Seed);
            tournament.AddTeam(team);
        }

        return tournament;
    }

    private TournamentFormat MapToTournamentFormat(FormatConfiguration formatConfig)
    {
        return formatConfig.Type.ToLowerInvariant() switch
        {
            "roundrobin" => TournamentFormat.RoundRobin(),
            "groups" or "groupstage" => TournamentFormat.Groups(
                formatConfig.GroupStage!.NumberOfGroups,
                formatConfig.GroupStage.TeamsAdvancingPerGroup ?? 1),
            "knockout" => TournamentFormat.Knockout(),
            _ => throw new ArgumentException($"Unknown format type: {formatConfig.Type}")
        };
    }

    private List<Field> MapToFields(List<FieldDefinition> fieldDefinitions)
    {
        var fields = new List<Field>();

        foreach (var fieldDef in fieldDefinitions)
        {
            var timeSlots = fieldDef.AvailabilityWindows
                .Select(w => new TimeSlot(w.Start, w.End))
                .ToList();

            var field = new Field(fieldDef.Name, timeSlots);
            fields.Add(field);
        }

        return fields;
    }

    private ScheduleOutput MapToScheduleOutput(string tournamentName, Schedule schedule)
    {
        var scheduledMatches = new List<ScheduledMatchDto>();

        foreach (var match in schedule.ScheduledMatches.Where(m => m.IsScheduled))
        {
            // TODO: Resolve team names from tournament
            scheduledMatches.Add(new ScheduledMatchDto
            {
                MatchId = (int)match.Id.GetHashCode(),
                HomeTeam = match.TeamAId.ToString(),
                AwayTeam = match.TeamBId.ToString(),
                Field = match.AssignedFieldId?.ToString() ?? "Unknown",
                StartTime = match.AssignedTimeSlot!.Start,
                EndTime = match.AssignedTimeSlot.End,
                Stage = match.Stage.ToString(),
                Round = match.RoundNumber
            });
        }

        var diagnostics = new ScheduleDiagnostics
        {
            IsValid = schedule.IsFeasible,
            TotalMatches = schedule.ScheduledMatches.Count(),
            HardConstraintViolations = 0, // TODO: Calculate from constraint violations
            SoftConstraintViolations = 0,
            Violations = new List<string>(),
            Warnings = new List<string>()
        };

        return new ScheduleOutput
        {
            TournamentName = tournamentName,
            Matches = scheduledMatches,
            Diagnostics = diagnostics,
            GeneratedAt = DateTime.UtcNow
        };
    }
}
