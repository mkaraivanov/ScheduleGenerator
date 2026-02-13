# Tournament Schedule Generator - JSON Configuration

## Overview

The Tournament Schedule Generator now supports two ways to run:

1. **Interactive Wizard** (original) - Step-by-step prompts for all configuration
2. **JSON Configuration** (new) - Load all settings from a JSON file

## Using JSON Configuration

### Quick Start

1. Create a JSON configuration file (or use one of the samples):
   - `tournament-config-simple.json` - Minimal 4-team tournament
   - `tournament-config.sample.json` - Full-featured tournament with comments

2. Run the generator:
   ```bash
   dotnet run --project src/ScheduleGenerator.Console -- generate-from-file -c <config-file>
   ```

3. Optionally save the output:
   ```bash
   dotnet run --project src/ScheduleGenerator.Console -- generate-from-file -c <config-file> -o output.json
   ```

### Command Help

```bash
# See all commands
dotnet run --project src/ScheduleGenerator.Console -- --help

# See JSON configuration options
dotnet run --project src/ScheduleGenerator.Console -- generate-from-file --help
```

## JSON Configuration Format

### Minimal Configuration

```json
{
  "name": "My Tournament",
  "teams": [
    { "name": "Team A" },
    { "name": "Team B" },
    { "name": "Team C" },
    { "name": "Team D" }
  ],
  "fields": [
    {
      "name": "Main Field",
      "availabilityWindows": [
        {
          "start": "2026-06-20T09:00:00",
          "end": "2026-06-20T17:00:00"
        }
      ]
    }
  ],
  "format": {
    "type": "RoundRobin"
  },
  "rules": {
    "matchDurationMinutes": 60,
    "bufferBetweenMatchesMinutes": 10,
    "minimumRestTimeMinutes": 60
  },
  "constraints": {
    "balancedKickoffTimesWeight": 1,
    "minimizeFieldChangesWeight": 1,
    "opponentSpacingWeight": 1,
    "seededTeamSeparationWeight": 1
  }
}
```

### Configuration Reference

#### Tournament Definition

- **name** (string, required): Tournament name
- **teams** (array, required): List of teams
- **fields** (array, required): Available fields with time windows
- **format** (object, required): Tournament format configuration
- **rules** (object, required): Scheduling rules
- **constraints** (object, optional): Soft constraint weights

#### Teams

```json
{
  "name": "Team Name",
  "seed": 1  // Optional: Used for seeding in group assignments
}
```

#### Fields

```json
{
  "name": "Field Name",
  "availabilityWindows": [
    {
      "start": "2026-06-20T09:00:00",
      "end": "2026-06-20T17:00:00"
    }
  ]
}
```

Multiple availability windows can be specified for each field.

#### Format Types

**Round Robin:**
```json
{
  "type": "RoundRobin"
}
```

**Group Stage:**
```json
{
  "type": "Groups",
  "groupStage": {
    "numberOfGroups": 2,
    "teamsAdvancingPerGroup": 2,
    "advancement": {
      "nextStage": "Knockout",
      "teamsAdvancing": 4
    }
  }
}
```

**Knockout:**
```json
{
  "type": "Knockout",
  "knockout": {
    "includeThirdPlaceMatch": true
  }
}
```

#### Rules

- **matchDurationMinutes** (int, required): Length of each match
- **bufferBetweenMatchesMinutes** (int, required): Time between matches on same field
- **minimumRestTimeMinutes** (int, optional): Minimum rest time for teams between matches
- **maxMatchesPerTeamPerDay** (int, optional): Maximum matches per team per day
- **minimumOpponentSpacingMinutes** (int, optional): Minimum time between rematches

#### Constraints

Soft constraint weights (higher = more important, 0 = disabled):

- **balancedKickoffTimesWeight**: Balance kickoff times across teams
- **minimizeFieldChangesWeight**: Keep teams on same field when possible
- **opponentSpacingWeight**: Maximize time between matches against same opponent
- **seededTeamSeparationWeight**: Separate seeded teams in schedule
- **customWeights**: Dictionary of custom constraint weights (optional)

## Examples

### Simple Weekend Tournament

```json
{
  "name": "Saturday Soccer",
  "teams": [
    { "name": "Reds" },
    { "name": "Blues" },
    { "name": "Greens" },
    { "name": "Yellows" }
  ],
  "fields": [
    {
      "name": "Field 1",
      "availabilityWindows": [
        { "start": "2026-06-20T09:00:00", "end": "2026-06-20T17:00:00" }
      ]
    }
  ],
  "format": { "type": "RoundRobin" },
  "rules": {
    "matchDurationMinutes": 45,
    "bufferBetweenMatchesMinutes": 10,
    "minimumRestTimeMinutes": 45
  },
  "constraints": {
    "balancedKickoffTimesWeight": 1,
    "minimizeFieldChangesWeight": 0,
    "opponentSpacingWeight": 2,
    "seededTeamSeparationWeight": 0
  }
}
```

### Multi-Day Tournament with Groups

```json
{
  "name": "Regional Championship",
  "teams": [
    { "name": "Team A", "seed": 1 },
    { "name": "Team B", "seed": 2 },
    { "name": "Team C", "seed": 3 },
    { "name": "Team D", "seed": 4 },
    { "name": "Team E" },
    { "name": "Team F" },
    { "name": "Team G" },
    { "name": "Team H" }
  ],
  "fields": [
    {
      "name": "Field 1",
      "availabilityWindows": [
        { "start": "2026-06-15T09:00:00", "end": "2026-06-15T18:00:00" },
        { "start": "2026-06-16T09:00:00", "end": "2026-06-16T18:00:00" }
      ]
    },
    {
      "name": "Field 2",
      "availabilityWindows": [
        { "start": "2026-06-15T10:00:00", "end": "2026-06-15T17:00:00" },
        { "start": "2026-06-16T10:00:00", "end": "2026-06-16T17:00:00" }
      ]
    }
  ],
  "format": {
    "type": "Groups",
    "groupStage": {
      "numberOfGroups": 2,
      "teamsAdvancingPerGroup": 2
    }
  },
  "rules": {
    "matchDurationMinutes": 90,
    "bufferBetweenMatchesMinutes": 15,
    "minimumRestTimeMinutes": 120,
    "maxMatchesPerTeamPerDay": 2
  },
  "constraints": {
    "balancedKickoffTimesWeight": 2,
    "minimizeFieldChangesWeight": 1,
    "opponentSpacingWeight": 3,
    "seededTeamSeparationWeight": 2
  }
}
```

## Benefits of JSON Configuration

- **Reproducibility**: Save and reuse tournament configurations
- **Version Control**: Track changes to tournament setup over time
- **Batch Processing**: Generate schedules for multiple similar tournaments
- **Easier Testing**: Quickly test different constraint weights and rules
- **Documentation**: Configuration files serve as tournament documentation
- **Automation**: Integrate with CI/CD pipelines or other tools

## Comparison: Wizard vs JSON Configuration

| Feature | Interactive Wizard | JSON Configuration |
|---------|-------------------|-------------------|
| Ease of use | ✅ Guided step-by-step | ⚠️ Requires JSON knowledge |
| Speed | ⚠️ Slower for complex tournaments | ✅ Instant with saved config |
| Repeatability | ❌ Must re-enter all data | ✅ Reusable configuration |
| Version Control | ❌ Not easily tracked | ✅ Git-friendly |
| Automation | ❌ Interactive only | ✅ Script-friendly |
| Documentation | ⚠️ No record of settings | ✅ Self-documenting |

## Tips

1. **Start with a sample**: Copy `tournament-config-simple.json` and modify it
2. **Use comments**: JSON5-style comments are supported in config files
3. **Validate before running**: The tool validates your configuration before scheduling
4. **Save successful configs**: Keep configurations that work well for future use
5. **Experiment with weights**: Try different constraint weights to optimize your schedule
6. **Test with simple configs**: Start small and add complexity gradually

## Troubleshooting

### Configuration file not found
- Check the path to your JSON file
- Use absolute paths or run from the project root directory

### JSON parsing errors
- Validate your JSON syntax (use a JSON validator)
- Remove trailing commas (unless using JSON5 comments)
- Check date format: `YYYY-MM-DDTHH:mm:ss`

### Validation errors
- Ensure all required fields are present
- Check that dates are in ISO 8601 format
- Verify teams and fields arrays are not empty
- Make sure field availability windows don't overlap incorrectly

### No matches scheduled
- Check that fields have sufficient availability windows
- Verify match duration + buffer time fits in availability windows
- Ensure there's enough time for all required matches
- Review constraint weights - overly strict constraints may prevent scheduling

## Further Reading

- See `Phase1-Complete.md` through `Phase4-Complete.md` for architecture details
- Check `Claude.md` for project overview and development approach
