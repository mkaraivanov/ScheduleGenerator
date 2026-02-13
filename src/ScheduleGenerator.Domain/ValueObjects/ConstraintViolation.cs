namespace ScheduleGenerator.Domain.ValueObjects;

/// <summary>
/// Represents a constraint violation with details about the issue.
/// </summary>
public record ConstraintViolation
{
    public string ConstraintName { get; init; }
    public string Description { get; init; }
    public ConstraintSeverity Severity { get; init; }
    public IReadOnlyCollection<Guid> AffectedEntityIds { get; init; }
    public bool IsHardConstraint { get; init; }
    public double Penalty { get; init; }
    public bool IsSatisfied { get; init; }

    public ConstraintViolation(
        string constraintName,
        string description,
        ConstraintSeverity severity,
        bool isHardConstraint = true,
        double penalty = 0,
        IEnumerable<Guid>? affectedEntityIds = null)
    {
        if (string.IsNullOrWhiteSpace(constraintName))
        {
            throw new ArgumentException("Constraint name cannot be empty.", nameof(constraintName));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Description cannot be empty.", nameof(description));
        }

        ConstraintName = constraintName;
        Description = description;
        Severity = severity;
        IsHardConstraint = isHardConstraint;
        Penalty = penalty;
        IsSatisfied = false;
        AffectedEntityIds = affectedEntityIds?.ToList() ?? new List<Guid>();
    }

    /// <summary>
    /// Creates a satisfied constraint violation (no actual violation).
    /// </summary>
    public static ConstraintViolation Satisfied(string? constraintName = null)
    {
        return new ConstraintViolation(
            constraintName ?? "No Violation",
            "Constraint is satisfied",
            ConstraintSeverity.Info,
            isHardConstraint: false,
            penalty: 0)
        {
            IsSatisfied = true
        };
    }

    /// <summary>
    /// Gets a formatted string representation of the violation.
    /// </summary>
    public override string ToString()
    {
        var entityInfo = AffectedEntityIds.Any() 
            ? $" (Affected entities: {AffectedEntityIds.Count})" 
            : string.Empty;
        return $"[{Severity}] {ConstraintName}: {Description}{entityInfo}";
    }
}

/// <summary>
/// Severity levels for constraint violations.
/// </summary>
public enum ConstraintSeverity
{
    /// <summary>
    /// Critical violation - schedule is invalid.
    /// </summary>
    Critical,

    /// <summary>
    /// Warning - schedule is valid but not optimal.
    /// </summary>
    Warning,

    /// <summary>
    /// Information - minor issue or suggestion.
    /// </summary>
    Info
}
