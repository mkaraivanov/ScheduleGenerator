using Microsoft.Extensions.Logging;
using ScheduleGenerator.Application.Interfaces;
using ScheduleGenerator.Domain.Constraints;
using ScheduleGenerator.Domain.Entities;
using ScheduleGenerator.Domain.ValueObjects;
using ScheduleGenerator.Infrastructure.Constraints;
using ApplicationSlot = ScheduleGenerator.Application.Models.Slot;

namespace ScheduleGenerator.Infrastructure.Algorithms;

/// <summary>
/// Implements constraint satisfaction scheduling using backtracking with MRV and LCV heuristics.
/// </summary>
public class BacktrackingScheduler : ISchedulingEngine
{
    private readonly ILogger<BacktrackingScheduler>? _logger;
    private ConstraintEvaluator? _evaluator;
    private List<Match>? _unassignedMatches;
    private Dictionary<Guid, ApplicationSlot>? _slotLookup;
    private Tournament? _tournament;
    private int _backtrackCount;

    public BacktrackingScheduler(ILogger<BacktrackingScheduler>? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// Schedules matches using backtracking algorithm.
    /// </summary>
    public async Task<Schedule> ScheduleAsync(
        IEnumerable<Match> matches,
        IEnumerable<ApplicationSlot> slots,
        IEnumerable<IConstraint> constraints,
        CancellationToken cancellationToken = default)
    {
        return await ScheduleAsync(matches, slots, constraints, null, cancellationToken);
    }

    /// <summary>
    /// Schedules matches using backtracking algorithm with tournament context.
    /// </summary>
    public async Task<Schedule> ScheduleAsync(
        IEnumerable<Match> matches,
        IEnumerable<ApplicationSlot> slots,
        IEnumerable<IConstraint> constraints,
        Tournament? tournament,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(matches);
        ArgumentNullException.ThrowIfNull(slots);
        ArgumentNullException.ThrowIfNull(constraints);

        var matchesList = matches.ToList();
        var slotsList = slots.ToList();
        
        _logger?.LogInformation(
            "Starting backtracking scheduler with {MatchCount} matches and {SlotCount} slots",
            matchesList.Count,
            slotsList.Count);

        // Store tournament context
        _tournament = tournament;

        // Initialize constraint evaluator
        var registry = new ConstraintRegistry();
        registry.RegisterRange(constraints);
        _evaluator = new ConstraintEvaluator(registry, tournament);

        // Create slot lookup for efficient access
        _slotLookup = slotsList
            .Select((slot, index) => new { Slot = slot, Id = Guid.NewGuid() })
            .ToDictionary(x => x.Id, x => x.Slot);

        // Initialize state
        var state = new ScheduleState(matchesList, slotsList);
        _unassignedMatches = new List<Match>(matchesList);
        _backtrackCount = 0;

        // Create initial schedule
        var schedule = new Schedule(tournament?.Id ?? Guid.NewGuid());

        // Run backtracking
        var success = await Task.Run(
            () => Backtrack(schedule, state, cancellationToken),
            cancellationToken);

        _logger?.LogInformation(
            "Backtracking completed: success={Success}, scheduled={ScheduledCount}, backtracks={BacktrackCount}",
            success, schedule.ScheduledMatches.Count(), _backtrackCount);

        if (success)
        {
            _logger?.LogInformation(
                "Successfully scheduled all matches with {BacktrackCount} backtracks",
                _backtrackCount);
        }
        else
        {
            schedule.MarkAsInfeasible("Unable to find a valid schedule satisfying all hard constraints");
            _logger?.LogWarning(
                "Failed to schedule matches after {BacktrackCount} backtracks",
                _backtrackCount);
        }

        // Evaluate final schedule
        var violations = _evaluator.Evaluate(schedule);
        foreach (var violation in violations)
        {
            schedule.AddViolation(violation);
        }

        return schedule;
    }

    /// <summary>
    /// Recursive backtracking algorithm.
    /// </summary>
    private bool Backtrack(
        Schedule schedule,
        ScheduleState state,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return false;

        // Base case: all matches assigned
        if (_unassignedMatches!.Count == 0)
        {
            return _evaluator!.SatisfiesHardConstraints(schedule);
        }

        // Select most constrained variable (MRV heuristic)
        var match = SelectMostConstrainedMatch(state);
        if (match == null)
            return false;

        _unassignedMatches.Remove(match);

        // Order slots by least constraining value (LCV heuristic)
        var orderedSlots = OrderSlotsByConstraintImpact(match, state);

        foreach (var (slotId, slot) in orderedSlots)
        {
            if (cancellationToken.IsCancellationRequested)
                return false;

            // Skip if slot is already used
            if (state.IsSlotUsed(slotId))
                continue;

            // Try assigning match to this slot
            AssignMatchToSlot(match, slot, slotId, schedule, state);

            // Check hard constraints
            if (!_evaluator!.ViolatesHardConstraints(schedule, match))
            {
                // Forward check
                if (ForwardCheck(state, match, slotId))
                {
                    // Recurse
                    if (Backtrack(schedule, state, cancellationToken))
                        return true;
                }
            }

            // Backtrack
            _backtrackCount++;
            UnassignMatch(match, slotId, schedule, state);
        }

        // Restore match to unassigned list
        _unassignedMatches.Add(match);
        return false;
    }

    /// <summary>
    /// Selects the match with the fewest feasible slots (MRV heuristic).
    /// </summary>
    private Match? SelectMostConstrainedMatch(ScheduleState state)
    {
        Match? selectedMatch = null;
        int minFeasibleSlots = int.MaxValue;

        foreach (var match in _unassignedMatches!)
        {
            var feasibleCount = CountFeasibleSlots(match, state);
            
            if (feasibleCount == 0)
            {
                // No feasible slots for this match - dead end
                return null;
            }

            if (feasibleCount < minFeasibleSlots)
            {
                minFeasibleSlots = feasibleCount;
                selectedMatch = match;
            }
        }

        return selectedMatch;
    }

    /// <summary>
    /// Counts feasible slots for a match.
    /// </summary>
    private int CountFeasibleSlots(Match match, ScheduleState state)
    {
        int count = 0;
        
        foreach (var slotId in _slotLookup!.Keys)
        {
            if (!state.IsSlotUsed(slotId))
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>
    /// Orders slots by their constraint impact (LCV heuristic).
    /// Slots that leave the most options for other matches are preferred.
    /// </summary>
    private List<(Guid slotId, ApplicationSlot slot)> OrderSlotsByConstraintImpact(
        Match match,
        ScheduleState state)
    {
        var slotImpacts = new List<(Guid slotId, ApplicationSlot slot, int impact)>();

        foreach (var (slotId, slot) in _slotLookup!)
        {
            if (state.IsSlotUsed(slotId))
                continue;

            // Calculate how many constraints this slot would create
            int impact = CalculateConstraintImpact(match, slot, state);
            slotImpacts.Add((slotId, slot, impact));
        }

        // Sort by ascending impact (least constraining first)
        return slotImpacts
            .OrderBy(x => x.impact)
            .Select(x => (x.slotId, x.slot))
            .ToList();
    }

    /// <summary>
    /// Calculates how many other matches would be constrained by this assignment.
    /// </summary>
    private int CalculateConstraintImpact(Match match, ApplicationSlot slot, ScheduleState state)
    {
        int impact = 0;

        // Check impact on other unassigned matches
        foreach (var otherMatch in _unassignedMatches!)
        {
            if (otherMatch.Id == match.Id)
                continue;

            // If the other match involves the same teams
            if (otherMatch.TeamAId == match.TeamAId ||
                otherMatch.TeamAId == match.TeamBId ||
                otherMatch.TeamBId == match.TeamAId ||
                otherMatch.TeamBId == match.TeamBId)
            {
                // This slot would constrain slots for the other match
                impact++;
            }

            // If the other match uses the same field
            // (need to check time overlap)
            impact++;
        }

        return impact;
    }

    /// <summary>
    /// Forward checking: updates feasible slots for remaining matches.
    /// </summary>
    private bool ForwardCheck(ScheduleState state, Match assignedMatch, Guid assignedSlotId)
    {
        var assignedSlot = _slotLookup![assignedSlotId];

        foreach (var match in _unassignedMatches!)
        {
            if (match.Id == assignedMatch.Id)
                continue;

            // Remove conflicting slots
            var teamsInvolvedInAssignedMatch = new[] { assignedMatch.TeamAId, assignedMatch.TeamBId };
            var teamsInvolvedInMatch = new[] { match.TeamAId, match.TeamBId };

            // If matches share a team, they can't be at overlapping times
            if (teamsInvolvedInAssignedMatch.Intersect(teamsInvolvedInMatch).Any())
            {
                foreach (var (slotId, slot) in _slotLookup)
                {
                    if (slot.OverlapsWith(assignedSlot))
                    {
                        state.RemoveFeasibleSlot(match.Id, slotId);
                    }
                }
            }

            // Check if match still has feasible slots
            if (CountFeasibleSlots(match, state) == 0)
            {
                return false; // Dead end detected
            }
        }

        return true;
    }

    /// <summary>
    /// Assigns a match to a slot.
    /// </summary>
    private void AssignMatchToSlot(
        Match match,
        ApplicationSlot slot,
        Guid slotId,
        Schedule schedule,
        ScheduleState state)
    {
        var timeSlot = new TimeSlot(slot.StartTime, slot.EndTime);
        match.AssignSlot(slotId, slot.Field.Id, timeSlot);
        state.AssignMatch(match, slot, slotId);
        schedule.AddMatch(match);
    }

    /// <summary>
    /// Removes a match assignment (backtracking).
    /// </summary>
    private void UnassignMatch(
        Match match,
        Guid slotId,
        Schedule schedule,
        ScheduleState state)
    {
        match.ClearAssignment();
        state.UnassignMatch(match, slotId);
        schedule.RemoveMatch(match.Id);
    }
}
