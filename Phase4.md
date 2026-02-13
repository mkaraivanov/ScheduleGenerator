# Phase 4: Interactive CLI with System.CommandLine

**Goal**: Build rich interactive wizard using System.CommandLine for tournament setup.

## Tasks

### 4.1 Console Entry Point
**Location**: `src/ScheduleGenerator.Console/Program.cs`

Set up:
- `HostApplicationBuilder` for dependency injection
- Register services from all layers (Domain, Application, Infrastructure)
- Configure Serilog with Console sink (colored, structured)
- Load `appsettings.json` configuration
- Create System.CommandLine root command
- Global exception handler with friendly error messages

### 4.2 Interactive Wizard Commands
**Location**: `src/ScheduleGenerator.Console/Commands/`

Implement `GenerateScheduleCommand`:
- Root command with description
- Wizard flow (step-by-step):
  1. Collect teams (loop until done)
  2. Define fields with availability
  3. Set time windows and blackouts
  4. Select tournament format
  5. Configure match rules (duration, rest time)
  6. Enable/configure constraints
  7. Execute scheduling
  8. Display results

Use System.CommandLine features:
- Interactive prompts with validation
- Progress reporting during scheduling
- Cancellation support (Ctrl+C)

### 4.3 Console UI Helpers
**Location**: `src/ScheduleGenerator.Console/UI/`

Implement collectors:
- `TeamCollector`: prompt for team name, optional club/seed, loop for multiple
- `FieldCollector`: field ID, availability windows (start/end datetime), multiple windows per field
- `TimeWindowCollector`: day boundaries, blackout periods
- `FormatSelector`: choose format type, configure format-specific options (groups count, etc.)
- `RulesConfigurator`: match/buffer minutes, rest time, max matches per day
- `ConstraintConfigurator`: enable/disable soft constraints, set weights

Implement output:
- `OutputRenderer`: format schedule as table (time | field | team A vs team B)
- Export schedule to console-friendly format
- Display diagnostics section (warnings, errors, suggestions)
- Color code violations (red = hard, yellow = soft)

## Tests to Write First (TDD)

- `GenerateScheduleCommandTests`: mock user input, verify flow
- `TeamCollectorTests`: parse valid/invalid input
- `FieldCollectorTests`: datetime parsing, validation
- `FormatSelectorTests`: all format types selectable
- `OutputRendererTests`: correct formatting, color codes

## Success Criteria

- Console app runs without errors
- Wizard guides user through all inputs with clear prompts
- Invalid inputs rejected with helpful messages
- Schedule output is readable and well-formatted
- Diagnostics explain any issues clearly
- Progress shown during long-running scheduling
- Graceful shutdown on Ctrl+C
