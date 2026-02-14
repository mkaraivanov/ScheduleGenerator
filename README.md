# Tournament Schedule Generator

A modern .NET 9 application for constraint-based tournament scheduling using clean architecture principles.

## Quick Start

### Option 1: Web Interface (Easiest - NEW!)

```bash
dotnet run --project src/ScheduleGenerator.Web
```

Open your browser to `http://localhost:5000` and configure your tournament using the intuitive web interface. Features:
- Interactive form-based configuration
- Real-time validation
- Visual schedule display
- Import/Export JSON configurations
- Download generated schedules

### Option 2: Interactive Wizard

```bash
dotnet run --project src/ScheduleGenerator.Console
```

Follow the step-by-step prompts to configure your tournament.

### Option 3: JSON Configuration (Fast & Repeatable)

```bash
dotnet run --project src/ScheduleGenerator.Console -- generate-from-file -c tournament-config.json
```

Use a JSON file to define your entire tournament configuration. Perfect for:
- Reproducible schedules
- Testing different configurations
- Automation and CI/CD integration
- Version-controlled tournament setups

**Sample configurations included:**
- `tournament-config-simple.json` - Basic 4-team tournament
- `tournament-config.sample.json` - Full-featured example with comments

## Features

- **Multiple Tournament Formats**: Round-robin, group stage, knockout
- **Flexible Constraints**: Hard constraints (must satisfy) + soft constraints (optimize)
- **Smart Scheduling**: Backtracking algorithm with CSP heuristics
- **Field Management**: Multiple fields with custom availability windows
- **Team Considerations**: Rest times, match limits, seeding support
- **Rich Output**: Console display + JSON export capability

## Documentation

- **[JSON-Configuration.md](JSON-Configuration.md)** - Complete JSON configuration guide with examples
- **[Claude.md](Claude.md)** - Architecture overview and tech stack
- **Phase1-7.md** - Detailed implementation phases

## Building & Testing

```bash
# Build the solution
dotnet build

# Run all tests
dotnet test

# See all available commands
dotnet run --project src/ScheduleGenerator.Console -- --help
```

## Architecture

Clean Architecture with clear separation of concerns:
- **Domain Layer**: Core entities, constraints, tournament formats
- **Application Layer**: Services, orchestration, validation
- **Infrastructure Layer**: Scheduling algorithms, slot generation
- **Console Layer**: Interactive CLI and JSON configuration support

## Requirements

- .NET 9 SDK
- No external dependencies required

## License

See LICENSE file for details.
