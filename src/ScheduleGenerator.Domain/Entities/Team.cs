namespace ScheduleGenerator.Domain.Entities;

/// <summary>
/// Represents a team participating in a tournament.
/// </summary>
public class Team
{
    public Guid Id { get; init; }
    public string Name { get; private set; }
    public string? Club { get; private set; }
    public int? Seed { get; private set; }
    public string? TravelConstraints { get; private set; }

    public Team(string name, string? club = null, int? seed = null, string? travelConstraints = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Team name cannot be empty.", nameof(name));
        }

        if (seed.HasValue && seed.Value < 1)
        {
            throw new ArgumentException("Seed must be a positive integer.", nameof(seed));
        }

        Id = Guid.NewGuid();
        Name = name;
        Club = club;
        Seed = seed;
        TravelConstraints = travelConstraints;
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Team name cannot be empty.", nameof(name));
        }
        Name = name;
    }

    public void UpdateClub(string? club)
    {
        Club = club;
    }

    public void UpdateSeed(int? seed)
    {
        if (seed.HasValue && seed.Value < 1)
        {
            throw new ArgumentException("Seed must be a positive integer.", nameof(seed));
        }
        Seed = seed;
    }

    public void UpdateTravelConstraints(string? travelConstraints)
    {
        TravelConstraints = travelConstraints;
    }

    public override string ToString() => Name;
}
