using ScheduleGenerator.Domain.ValueObjects;

namespace ScheduleGenerator.Domain.Entities;

/// <summary>
/// Represents a complete schedule with scheduled matches and diagnostics.
/// </summary>
public class Schedule
{
    private readonly List<Match> _scheduledMatches = new();
    private readonly List<ConstraintViolation> _violations = new();

    public Guid Id { get; init; }
    public Guid TournamentId { get; init; }
    public IReadOnlyCollection<Match> ScheduledMatches => _scheduledMatches.AsReadOnly();
    public IReadOnlyCollection<ConstraintViolation> Violations => _violations.AsReadOnly();
    public bool IsFeasible { get; private set; }
    public DateTime CreatedAt { get; init; }
    public DateTime? LastModifiedAt { get; private set; }

    public Schedule(Guid tournamentId)
    {
        if (tournamentId == Guid.Empty)
        {
            throw new ArgumentException("Tournament ID cannot be empty.", nameof(tournamentId));
        }

        Id = Guid.NewGuid();
        TournamentId = tournamentId;
        CreatedAt = DateTime.UtcNow;
        IsFeasible = true;
    }

    public void AddMatch(Match match)
    {
        ArgumentNullException.ThrowIfNull(match, nameof(match));

        if (_scheduledMatches.Any(m => m.Id == match.Id))
        {
            throw new InvalidOperationException($"Match with ID {match.Id} already exists in the schedule.");
        }

        _scheduledMatches.Add(match);
        LastModifiedAt = DateTime.UtcNow;
    }

    public void RemoveMatch(Guid matchId)
    {
        var match = _scheduledMatches.FirstOrDefault(m => m.Id == matchId);
        if (match != null)
        {
            _scheduledMatches.Remove(match);
            LastModifiedAt = DateTime.UtcNow;
        }
    }

    public void ClearMatches()
    {
        _scheduledMatches.Clear();
        LastModifiedAt = DateTime.UtcNow;
    }

    public void AddViolation(ConstraintViolation violation)
    {
        ArgumentNullException.ThrowIfNull(violation, nameof(violation));
        _violations.Add(violation);
        
        // If any critical violation exists, schedule is not feasible
        if (violation.Severity == ConstraintSeverity.Critical)
        {
            IsFeasible = false;
        }
        
        LastModifiedAt = DateTime.UtcNow;
    }

    public void ClearViolations()
    {
        _violations.Clear();
        IsFeasible = true;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void SetFeasibility(bool isFeasible)
    {
        IsFeasible = isFeasible;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void MarkAsInfeasible(string reason)
    {
        IsFeasible = false;
        AddViolation(new ConstraintViolation(
            "Infeasible Schedule",
            reason,
            ConstraintSeverity.Critical,
            isHardConstraint: true,
            penalty: 0));
    }

    /// <summary>
    /// Gets all matches involving a specific team.
    /// </summary>
    public IEnumerable<Match> GetMatchesForTeam(Guid teamId)
    {
        return _scheduledMatches.Where(m => m.InvolvesTeam(teamId));
    }

    /// <summary>
    /// Gets all matches scheduled on a specific field.
    /// </summary>
    public IEnumerable<Match> GetMatchesOnField(Guid fieldId)
    {
        return _scheduledMatches.Where(m => m.AssignedFieldId == fieldId);
    }

    /// <summary>
    /// Gets all matches scheduled during a specific time slot.
    /// </summary>
    public IEnumerable<Match> GetMatchesDuringTimeSlot(TimeSlot timeSlot)
    {
        return _scheduledMatches.Where(m => 
            m.AssignedTimeSlot != null && m.AssignedTimeSlot.OverlapsWith(timeSlot));
    }

    /// <summary>
    /// Gets the count of scheduled matches.
    /// </summary>
    public int ScheduledMatchCount => _scheduledMatches.Count(m => m.IsScheduled);

    /// <summary>
    /// Gets the count of unscheduled matches.
    /// </summary>
    public int UnscheduledMatchCount => _scheduledMatches.Count(m => !m.IsScheduled);

    /// <summary>
    /// Gets the count of critical violations.
    /// </summary>
    public int CriticalViolationCount => _violations.Count(v => v.Severity == ConstraintSeverity.Critical);

    /// <summary>
    /// Gets the count of warning violations.
    /// </summary>
    public int WarningViolationCount => _violations.Count(v => v.Severity == ConstraintSeverity.Warning);

    public override string ToString()
    {
        return $"Schedule: {ScheduledMatchCount} scheduled, {UnscheduledMatchCount} unscheduled, " +
               $"{CriticalViolationCount} critical violations, Feasible: {IsFeasible}";
    }
}
