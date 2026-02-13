using ScheduleGenerator.Domain.Constraints;
using ScheduleGenerator.Domain.Entities;
using ScheduleGenerator.Domain.ValueObjects;

namespace ScheduleGenerator.Infrastructure.Constraints;

/// <summary>
/// Registry for managing constraints with dynamic registration.
/// </summary>
public class ConstraintRegistry
{
    private readonly List<IConstraint> _hardConstraints = new();
    private readonly List<IConstraint> _softConstraints = new();

    public IReadOnlyList<IConstraint> HardConstraints => _hardConstraints.AsReadOnly();
    public IReadOnlyList<IConstraint> SoftConstraints => _softConstraints.AsReadOnly();
    public IReadOnlyList<IConstraint> AllConstraints => 
        _hardConstraints.Concat(_softConstraints).ToList().AsReadOnly();

    /// <summary>
    /// Registers a constraint.
    /// </summary>
    public void Register(IConstraint constraint)
    {
        ArgumentNullException.ThrowIfNull(constraint);

        if (constraint.Type == ConstraintType.Hard)
        {
            _hardConstraints.Add(constraint);
        }
        else
        {
            _softConstraints.Add(constraint);
        }
    }

    /// <summary>
    /// Registers multiple constraints.
    /// </summary>
    public void RegisterRange(IEnumerable<IConstraint> constraints)
    {
        foreach (var constraint in constraints)
        {
            Register(constraint);
        }
    }

    /// <summary>
    /// Clears all registered constraints.
    /// </summary>
    public void Clear()
    {
        _hardConstraints.Clear();
        _softConstraints.Clear();
    }
}

/// <summary>
/// Evaluates constraints and aggregates violations.
/// </summary>
public class ConstraintEvaluator
{
    private readonly ConstraintRegistry _registry;
    private readonly Dictionary<string, List<ConstraintViolation>> _violationCache = new();
    private Tournament? _tournamentContext;

    public ConstraintEvaluator(ConstraintRegistry registry, Tournament? tournament = null)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _tournamentContext = tournament;
    }

    public void SetTournamentContext(Tournament tournament)
    {
        _tournamentContext = tournament;
    }

    /// <summary>
    /// Evaluates all constraints for a schedule and returns violations.
    /// </summary>
    public IReadOnlyList<ConstraintViolation> Evaluate(Schedule schedule)
    {
        ArgumentNullException.ThrowIfNull(schedule);

        var violations = new List<ConstraintViolation>();

        foreach (var constraint in _registry.AllConstraints)
        {
            var results = constraint.Evaluate(schedule, _tournamentContext!);
            violations.AddRange(results);
        }

        return violations;
    }

    /// <summary>
    /// Checks if a specific match assignment violates any hard constraints.
    /// </summary>
    public bool ViolatesHardConstraints(Schedule schedule, Match match)
    {
        ArgumentNullException.ThrowIfNull(schedule);
        ArgumentNullException.ThrowIfNull(match);

        foreach (var constraint in _registry.HardConstraints)
        {
            var results = constraint.Evaluate(schedule, _tournamentContext!);
            if (results.Any())
            {
                // System.Console.WriteLine($"  Hard constraint violated: {constraint.GetType().Name} - {results.Count} violation(s)");
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Calculates the total penalty from soft constraint violations.
    /// </summary>
    public double CalculateTotalPenalty(Schedule schedule)
    {
        ArgumentNullException.ThrowIfNull(schedule);

        double totalPenalty = 0;

        foreach (var constraint in _registry.SoftConstraints)
        {
            var results = constraint.Evaluate(schedule, _tournamentContext!);
            foreach (var violation in results)
            {
                totalPenalty += violation.Penalty;
            }
        }

        return totalPenalty;
    }

    /// <summary>
    /// Checks if a schedule satisfies all hard constraints.
    /// </summary>
    public bool SatisfiesHardConstraints(Schedule schedule)
    {
        ArgumentNullException.ThrowIfNull(schedule);

        foreach (var constraint in _registry.HardConstraints)
        {
            var results = constraint.Evaluate(schedule, _tournamentContext!);
            if (results.Any())
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Clears the violation cache.
    /// </summary>
    public void ClearCache()
    {
        _violationCache.Clear();
    }
}
