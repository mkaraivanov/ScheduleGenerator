using ScheduleGenerator.Application.Models;
using ScheduleGenerator.Domain.Entities;

namespace ScheduleGenerator.Infrastructure.Algorithms;

/// <summary>
/// Represents the current state of scheduling during the backtracking process.
/// Tracks assigned matches, available slots, and team schedules.
/// </summary>
public class ScheduleState
{
    private readonly Dictionary<Guid, Slot> _matchAssignments = new();
    private readonly HashSet<Guid> _usedSlotIds = new();
    private readonly Dictionary<Guid, List<Slot>> _teamSchedules = new();
    private readonly Dictionary<Guid, HashSet<Guid>> _feasibleSlots = new();
    private readonly List<Slot> _allSlots;

    public IReadOnlyDictionary<Guid, Slot> MatchAssignments => _matchAssignments;
    public IReadOnlyCollection<Guid> UsedSlotIds => _usedSlotIds;

    public ScheduleState(IEnumerable<Match> matches, IEnumerable<Slot> slots)
    {
        _allSlots = slots.ToList();
        
        // Initialize feasible slots for each match (all slots initially feasible)
        foreach (var match in matches)
        {
            _feasibleSlots[match.Id] = _allSlots.Select((s, i) => (Guid)Guid.NewGuid()).ToHashSet();
            
            // Initialize team schedules
            if (!_teamSchedules.ContainsKey(match.TeamAId))
                _teamSchedules[match.TeamAId] = new List<Slot>();
            if (!_teamSchedules.ContainsKey(match.TeamBId))
                _teamSchedules[match.TeamBId] = new List<Slot>();
        }
    }

    /// <summary>
    /// Assigns a match to a slot.
    /// </summary>
    public void AssignMatch(Match match, Slot slot, Guid slotId)
    {
        _matchAssignments[match.Id] = slot;
        _usedSlotIds.Add(slotId);
        
        _teamSchedules[match.TeamAId].Add(slot);
        _teamSchedules[match.TeamBId].Add(slot);
    }

    /// <summary>
    /// Removes the assignment for a match (used during backtracking).
    /// </summary>
    public void UnassignMatch(Match match, Guid slotId)
    {
        if (_matchAssignments.Remove(match.Id, out var slot))
        {
            _usedSlotIds.Remove(slotId);
            _teamSchedules[match.TeamAId].Remove(slot);
            _teamSchedules[match.TeamBId].Remove(slot);
        }
    }

    /// <summary>
    /// Gets all slots assigned to a specific team.
    /// </summary>
    public IReadOnlyList<Slot> GetTeamSchedule(Guid teamId)
    {
        return _teamSchedules.TryGetValue(teamId, out var schedule) 
            ? schedule.AsReadOnly() 
            : Array.Empty<Slot>();
    }

    /// <summary>
    /// Checks if a slot is already used.
    /// </summary>
    public bool IsSlotUsed(Guid slotId)
    {
        return _usedSlotIds.Contains(slotId);
    }

    /// <summary>
    /// Gets the number of assigned matches.
    /// </summary>
    public int AssignedCount => _matchAssignments.Count;

    /// <summary>
    /// Gets feasible slots for a match.
    /// </summary>
    public IReadOnlySet<Guid> GetFeasibleSlots(Guid matchId)
    {
        return _feasibleSlots.TryGetValue(matchId, out var slots) 
            ? slots 
            : new HashSet<Guid>();
    }

    /// <summary>
    /// Updates feasible slots for a match (used in forward checking).
    /// </summary>
    public void UpdateFeasibleSlots(Guid matchId, IEnumerable<Guid> feasibleSlotIds)
    {
        _feasibleSlots[matchId] = feasibleSlotIds.ToHashSet();
    }

    /// <summary>
    /// Removes a slot from the feasible set of a match.
    /// </summary>
    public void RemoveFeasibleSlot(Guid matchId, Guid slotId)
    {
        if (_feasibleSlots.TryGetValue(matchId, out var slots))
        {
            slots.Remove(slotId);
        }
    }
}
