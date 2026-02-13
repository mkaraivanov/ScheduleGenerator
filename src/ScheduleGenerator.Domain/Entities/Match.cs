using ScheduleGenerator.Domain.ValueObjects;

namespace ScheduleGenerator.Domain.Entities;

/// <summary>
/// Represents a match between two teams.
/// </summary>
public class Match
{
    private readonly List<Guid> _prerequisiteMatchIds = new();

    public Guid Id { get; init; }
    public Guid TeamAId { get; private set; }
    public Guid TeamBId { get; private set; }
    public MatchStage Stage { get; private set; }
    public int RoundNumber { get; private set; }
    public string? GroupIdentifier { get; private set; }
    public IReadOnlyCollection<Guid> PrerequisiteMatchIds => _prerequisiteMatchIds.AsReadOnly();

    // Scheduling information
    public Guid? AssignedSlotId { get; private set; }
    public Guid? AssignedFieldId { get; private set; }
    public TimeSlot? AssignedTimeSlot { get; private set; }

    public Match(
        Guid teamAId,
        Guid teamBId,
        MatchStage stage,
        int roundNumber,
        string? groupIdentifier = null,
        IEnumerable<Guid>? prerequisiteMatchIds = null)
    {
        if (teamAId == Guid.Empty)
        {
            throw new ArgumentException("Team A ID cannot be empty.", nameof(teamAId));
        }

        if (teamBId == Guid.Empty)
        {
            throw new ArgumentException("Team B ID cannot be empty.", nameof(teamBId));
        }

        if (teamAId == teamBId)
        {
            throw new ArgumentException("A team cannot play against itself.");
        }

        if (roundNumber < 1)
        {
            throw new ArgumentException("Round number must be positive.", nameof(roundNumber));
        }

        Id = Guid.NewGuid();
        TeamAId = teamAId;
        TeamBId = teamBId;
        Stage = stage;
        RoundNumber = roundNumber;
        GroupIdentifier = groupIdentifier;

        if (prerequisiteMatchIds != null)
        {
            _prerequisiteMatchIds.AddRange(prerequisiteMatchIds);
        }
    }

    public void AssignSlot(Guid slotId, Guid fieldId, TimeSlot timeSlot)
    {
        ArgumentNullException.ThrowIfNull(timeSlot, nameof(timeSlot));

        if (slotId == Guid.Empty)
        {
            throw new ArgumentException("Slot ID cannot be empty.", nameof(slotId));
        }

        if (fieldId == Guid.Empty)
        {
            throw new ArgumentException("Field ID cannot be empty.", nameof(fieldId));
        }

        AssignedSlotId = slotId;
        AssignedFieldId = fieldId;
        AssignedTimeSlot = timeSlot;
    }

    public void UnassignSlot()
    {
        AssignedSlotId = null;
        AssignedFieldId = null;
        AssignedTimeSlot = null;
    }

    public void ClearAssignment() => UnassignSlot();

    public bool IsScheduled => AssignedSlotId.HasValue;

    public bool InvolvesTeam(Guid teamId)
    {
        return TeamAId == teamId || TeamBId == teamId;
    }

    public void AddPrerequisiteMatch(Guid matchId)
    {
        if (matchId == Guid.Empty)
        {
            throw new ArgumentException("Match ID cannot be empty.", nameof(matchId));
        }

        if (!_prerequisiteMatchIds.Contains(matchId))
        {
            _prerequisiteMatchIds.Add(matchId);
        }
    }

    public override string ToString()
    {
        var scheduled = IsScheduled ? $" at {AssignedTimeSlot?.Start}" : " (unscheduled)";
        return $"Match {TeamAId} vs {TeamBId} - Round {RoundNumber}{scheduled}";
    }
}

/// <summary>
/// Represents the stage of a match in a tournament.
/// </summary>
public enum MatchStage
{
    GroupStage,
    RoundOf32,
    RoundOf16,
    QuarterFinal,
    SemiFinal,
    ThirdPlace,
    Final,
    RoundRobin
}
