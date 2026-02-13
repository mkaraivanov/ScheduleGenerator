using ScheduleGenerator.Domain.ValueObjects;

namespace ScheduleGenerator.Domain.Entities;

/// <summary>
/// Represents a tournament with teams, fields, format, and scheduling rules.
/// </summary>
public class Tournament
{
    private readonly List<Team> _teams = new();
    private readonly List<Field> _fields = new();
    private readonly List<TimeWindow> _timeWindows = new();

    public Guid Id { get; init; }
    public string Name { get; private set; }
    public TournamentFormat Format { get; private set; }
    public MatchDuration MatchDuration { get; private set; }
    public IReadOnlyCollection<Team> Teams => _teams.AsReadOnly();
    public IReadOnlyCollection<Field> Fields => _fields.AsReadOnly();
    public IReadOnlyCollection<TimeWindow> TimeWindows => _timeWindows.AsReadOnly();
    public TimeSpan MinimumRestTime { get; private set; }

    public Tournament(
        string name,
        TournamentFormat format,
        MatchDuration matchDuration,
        TimeSpan? minimumRestTime = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Tournament name cannot be empty.", nameof(name));
        }

        ArgumentNullException.ThrowIfNull(format, nameof(format));
        ArgumentNullException.ThrowIfNull(matchDuration, nameof(matchDuration));

        Id = Guid.NewGuid();
        Name = name;
        Format = format;
        MatchDuration = matchDuration;
        MinimumRestTime = minimumRestTime ?? TimeSpan.FromHours(2);
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Tournament name cannot be empty.", nameof(name));
        }
        Name = name;
    }

    public void UpdateFormat(TournamentFormat format)
    {
        ArgumentNullException.ThrowIfNull(format, nameof(format));
        Format = format;
    }

    public void UpdateMatchDuration(MatchDuration matchDuration)
    {
        ArgumentNullException.ThrowIfNull(matchDuration, nameof(matchDuration));
        MatchDuration = matchDuration;
    }

    public void UpdateMinimumRestTime(TimeSpan minimumRestTime)
    {
        if (minimumRestTime < TimeSpan.Zero)
        {
            throw new ArgumentException("Minimum rest time cannot be negative.", nameof(minimumRestTime));
        }
        MinimumRestTime = minimumRestTime;
    }

    public void AddTeam(Team team)
    {
        ArgumentNullException.ThrowIfNull(team, nameof(team));
        
        if (_teams.Any(t => t.Id == team.Id))
        {
            throw new InvalidOperationException($"Team with ID {team.Id} already exists in the tournament.");
        }

        if (_teams.Any(t => t.Name.Equals(team.Name, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"Team with name '{team.Name}' already exists in the tournament.");
        }

        _teams.Add(team);
    }

    public void RemoveTeam(Guid teamId)
    {
        var team = _teams.FirstOrDefault(t => t.Id == teamId);
        if (team != null)
        {
            _teams.Remove(team);
        }
    }

    public void AddField(Field field)
    {
        ArgumentNullException.ThrowIfNull(field, nameof(field));
        
        if (_fields.Any(f => f.Id == field.Id))
        {
            throw new InvalidOperationException($"Field with ID {field.Id} already exists in the tournament.");
        }

        _fields.Add(field);
    }

    public void RemoveField(Guid fieldId)
    {
        var field = _fields.FirstOrDefault(f => f.Id == fieldId);
        if (field != null)
        {
            _fields.Remove(field);
        }
    }

    public void AddTimeWindow(TimeWindow timeWindow)
    {
        ArgumentNullException.ThrowIfNull(timeWindow, nameof(timeWindow));
        _timeWindows.Add(timeWindow);
    }

    public void RemoveTimeWindow(TimeWindow timeWindow)
    {
        _timeWindows.Remove(timeWindow);
    }

    public void ClearTimeWindows()
    {
        _timeWindows.Clear();
    }

    public override string ToString() => Name;
}

/// <summary>
/// Represents the format of a tournament.
/// </summary>
public class TournamentFormat
{
    public TournamentType Type { get; init; }
    public int Legs { get; init; }
    public int? GroupCount { get; init; }
    public int? TeamsAdvancingPerGroup { get; init; }
    public bool HasKnockoutStage { get; init; }

    public TournamentFormat(
        TournamentType type,
        int legs = 1,
        int? groupCount = null,
        int? teamsAdvancingPerGroup = null,
        bool hasKnockoutStage = false)
    {
        if (legs < 1)
        {
            throw new ArgumentException("Legs must be at least 1.", nameof(legs));
        }

        if (groupCount.HasValue && groupCount.Value < 1)
        {
            throw new ArgumentException("Group count must be positive.", nameof(groupCount));
        }

        if (teamsAdvancingPerGroup.HasValue && teamsAdvancingPerGroup.Value < 1)
        {
            throw new ArgumentException("Teams advancing per group must be positive.", nameof(teamsAdvancingPerGroup));
        }

        Type = type;
        Legs = legs;
        GroupCount = groupCount;
        TeamsAdvancingPerGroup = teamsAdvancingPerGroup;
        HasKnockoutStage = hasKnockoutStage;
    }

    public static TournamentFormat RoundRobin(int legs = 1) => new(TournamentType.RoundRobin, legs);
    
    public static TournamentFormat Groups(int groupCount, int teamsAdvancingPerGroup, bool hasKnockoutStage = false) 
        => new(TournamentType.GroupStage, 1, groupCount, teamsAdvancingPerGroup, hasKnockoutStage);
    
    public static TournamentFormat Knockout() => new(TournamentType.Knockout);
}

/// <summary>
/// Types of tournament formats.
/// </summary>
public enum TournamentType
{
    RoundRobin,
    GroupStage,
    Knockout,
    Hybrid
}
